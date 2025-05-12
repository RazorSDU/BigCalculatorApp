using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace BigCalculatorApp
{
    public class CalculatorForm : Form
    {
        private TableLayoutPanel mainLayout;    // Splits the entire form: top half for display, bottom half for buttons
        private TableLayoutPanel displayLayout; // Splits top half: row0 for calculation, row1 for result
        private TextBox txtCalculation;
        private Label lblResult;
        private TableLayoutPanel tableLayout;

        public CalculatorForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Big Calculator";
            this.Width = 450;
            this.Height = 450;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true; // So we can handle Enter/Escape in KeyDown

            // Overall mainLayout -> 2 rows, each 50%
            mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 2;
            mainLayout.ColumnCount = 1;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30f)); // top half
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70f)); // bottom half
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            // --------------------------------------------------
            // 1) DISPLAY LAYOUT (top half)
            //    Another TableLayoutPanel with 2 rows:
            //      row0 => calculation text box, row1 => result label
            // --------------------------------------------------
            displayLayout = new TableLayoutPanel();
            displayLayout.Dock = DockStyle.Fill;
            displayLayout.RowCount = 2;
            displayLayout.ColumnCount = 1;
            displayLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            displayLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            displayLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

            // Add some left/right padding around the top panel
            // so there's a 5px space on left/right edges.
            displayLayout.Margin = new Padding(5, 0, 5, 0);

            // Calculation textbox in row0
            txtCalculation = new TextBox();
            txtCalculation.Dock = DockStyle.Fill;
            txtCalculation.TextAlign = HorizontalAlignment.Right;
            txtCalculation.Font = new Font("Segoe UI", 18, FontStyle.Regular);
            txtCalculation.Multiline = true;    // so it can expand vertically
            txtCalculation.ScrollBars = ScrollBars.None;
            txtCalculation.BorderStyle = BorderStyle.None;
            txtCalculation.BackColor = Color.LightSteelBlue;
            txtCalculation.TabStop = true;

            // We'll handle KeyPress to filter allowed characters
            txtCalculation.KeyPress += TxtCalculation_KeyPress;

            // We'll handle Resize to recalc the "vertical center" hack
            txtCalculation.Resize += (s, e) => AdjustVerticalCenter();

            // Also handle text changes or font changed, if you want
            // e.g. txtCalculation.FontChanged or TextChanged => AdjustVerticalCenter()

            displayLayout.Controls.Add(txtCalculation, 0, 0);

            // The result label in row1
            lblResult = new Label();
            lblResult.Text = "0";
            lblResult.Dock = DockStyle.Fill;
            lblResult.TextAlign = ContentAlignment.MiddleRight;
            lblResult.Font = new Font("Segoe UI", 18, FontStyle.Regular);
            lblResult.AutoSize = false;
            lblResult.BackColor = Color.LightSteelBlue;
            displayLayout.Controls.Add(lblResult, 0, 1);

            mainLayout.Controls.Add(displayLayout, 0, 0);

            // --------------------------------------------------
            // 2) BUTTON LAYOUT (bottom half)
            // --------------------------------------------------
            tableLayout = new TableLayoutPanel();
            tableLayout.Dock = DockStyle.Fill;
            tableLayout.ColumnCount = 6;
            tableLayout.RowCount = 5;
            tableLayout.BackColor = Color.LightGray;

            for (int c = 0; c < 6; c++)
                tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.6f));
            for (int r = 0; r < 5; r++)
                tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

            // EXAMPLE layout (based on your note):
            // row0: 7, 8, 9, ÷, [null], Del
            // row1: 4, 5, 6, ×, [null], C
            // row2: 1, 2, 3, -, [null], null
            // row3: 0, ., ans, +, [null], null
            // row4: √, ^, (, ), [null], =
            string[,] buttonTexts = new string[5, 6]
            {
                { "7", "8", "9", "÷", null, "Del" },
                { "4", "5", "6", "×", null, "C" },
                { "1", "2", "3", "-", null, null },
                { "0", ".", "ans", "+", null, null },
                { "√", "^", "(", ")", null, "=" }
            };

            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 6; c++)
                {
                    string text = buttonTexts[r, c];
                    if (string.IsNullOrEmpty(text)) continue; // skip empty cells

                    Button btn = new Button();
                    btn.Text = text;
                    btn.Dock = DockStyle.Fill;
                    btn.Font = new Font("Segoe UI", 14, FontStyle.Regular);
                    btn.BackColor = Color.Gray;
                    btn.ForeColor = Color.White;
                    btn.Tag = text;
                    btn.Click += CalculatorButton_Click;
                    tableLayout.Controls.Add(btn, c, r);
                }
            }

            mainLayout.Controls.Add(tableLayout, 0, 1);

            // Add mainLayout to the form
            this.Controls.Add(mainLayout);

            // Handle special keys (Enter, Escape)
            this.KeyDown += CalculatorForm_KeyDown;

            // Refresh display on resize
            this.Resize += (s, e) => RefreshDisplay();
        }

        // ** Hack to simulate vertical centering in the TextBox **
        // We'll measure the font height, then set Padding.Top accordingly.
        private void AdjustVerticalCenter()
        {
            // We do this only if multiline is true
            // If single-line, there's no property for vertical alignment.
            if (!txtCalculation.Multiline) return;

            // Measure text height
            int fontHeight = TextRenderer.MeasureText("Xy", txtCalculation.Font).Height;
            int boxHeight = txtCalculation.ClientSize.Height;

            // We want to push the text down about half the leftover space
            int leftover = boxHeight - fontHeight;
            if (leftover < 0) leftover = 0;
            int topPadding = leftover / 2;

            // We'll keep a consistent left=5, right=5, bottom=0
            int left = 5, right = 5, bottom = 0;
            txtCalculation.Padding = new Padding(left, topPadding, right, bottom);
        }

        //
        // KEY PRESS FILTER: only allow calculator chars + a few control keys
        //
        private void TxtCalculation_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                // Let backspace, etc. pass
                return;
            }

            // Convert '*' => '×'
            if (e.KeyChar == '*')
            {
                e.Handled = true;
                InsertTextAtCaret("×");
                return;
            }
            // Convert '/' => '÷'
            if (e.KeyChar == '/')
            {
                e.Handled = true;
                InsertTextAtCaret("÷");
                return;
            }

            // Allowed characters: digits, +, -, ×, ÷, ^, ( ), ., √
            string allowed = "0123456789+-×÷^().√";

            if (!allowed.Contains(e.KeyChar))
            {
                // block it
                e.Handled = true;
            }
            // else allow
        }

        private void CalculatorButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string btnText = btn.Text;

            switch (btnText)
            {
                case "=":
                    lblResult.Text = EvaluateExpression(txtCalculation.Text);
                    txtCalculation.Focus();
                    return;

                case "C":
                    txtCalculation.Text = "";
                    lblResult.Text = "0";
                    txtCalculation.Focus();
                    return;

                case "ans":
                    if (lblResult.Text != "0" && lblResult.Text != "Error")
                    {
                        InsertTextAtCaret(lblResult.Text);
                    }
                    txtCalculation.Focus();
                    return;

                case "Del":
                    HandleBackspace();
                    txtCalculation.Focus();
                    return;

                default:
                    InsertTextAtCaret(btnText);
                    txtCalculation.Focus();
                    return;
            }
        }

        // Insert text at the caret (or replace selection)
        private void InsertTextAtCaret(string textToInsert)
        {
            int selStart = txtCalculation.SelectionStart;
            string original = txtCalculation.Text ?? "";

            if (txtCalculation.SelectionLength > 0)
            {
                int selLength = txtCalculation.SelectionLength;
                txtCalculation.Text = original.Remove(selStart, selLength)
                                              .Insert(selStart, textToInsert);
            }
            else
            {
                txtCalculation.Text = original.Insert(selStart, textToInsert);
            }

            txtCalculation.SelectionStart = selStart + textToInsert.Length;
            txtCalculation.SelectionLength = 0;

            // Re-adjust vertical center in case the text box changed size
            AdjustVerticalCenter();
        }

        // "Backspace" logic
        private void HandleBackspace()
        {
            int selStart = txtCalculation.SelectionStart;
            string original = txtCalculation.Text ?? "";

            if (txtCalculation.SelectionLength > 0)
            {
                int selLength = txtCalculation.SelectionLength;
                txtCalculation.Text = original.Remove(selStart, selLength);
                txtCalculation.SelectionStart = selStart;
                txtCalculation.SelectionLength = 0;
            }
            else
            {
                if (selStart > 0)
                {
                    txtCalculation.Text = original.Remove(selStart - 1, 1);
                    txtCalculation.SelectionStart = selStart - 1;
                    txtCalculation.SelectionLength = 0;
                }
            }

            AdjustVerticalCenter();
        }

        // Evaluate by calling BigCalcEngine
        private string EvaluateExpression(string expr)
        {
            if (string.IsNullOrWhiteSpace(expr))
                return "0";

            try
            {
                decimal result = BigCalcEngine.EvaluateExpression(expr);
                return result.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                return "Error";
            }
        }

        // Handle special keys: Enter => evaluate, Escape => clear
        private void CalculatorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                lblResult.Text = EvaluateExpression(txtCalculation.Text);
                txtCalculation.Focus();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                txtCalculation.Text = "";
                lblResult.Text = "0";
                txtCalculation.Focus();
                e.Handled = true;
            }
        }

        private void RefreshDisplay()
        {
            AdjustResultLabelFormat();
            // Also re-center the calculation text in case the form was resized
            AdjustVerticalCenter();
        }

        private void AdjustResultLabelFormat()
        {
            lblResult.Font = new Font(lblResult.Font.FontFamily, 18f, FontStyle.Regular);

            using (Graphics g = lblResult.CreateGraphics())
            {
                SizeF normalSize = g.MeasureString(lblResult.Text, lblResult.Font);
                if (normalSize.Width > lblResult.Width - 10)
                {
                    if (double.TryParse(lblResult.Text, out double dVal))
                    {
                        string sci = dVal.ToString("G6");
                        SizeF sciSize = g.MeasureString(sci, lblResult.Font);

                        int sigDigits = 6;
                        while (sciSize.Width > lblResult.Width - 10 && sigDigits > 1)
                        {
                            sigDigits--;
                            sci = dVal.ToString($"G{sigDigits}");
                            sciSize = g.MeasureString(sci, lblResult.Font);
                        }
                        lblResult.Text = sci;
                    }
                }
            }
        }
    }
}
