# BigCalculatorApp — precision desktop calculator  
> Desktop WinForms calculator that parses and evaluates complex arithmetic with high-precision decimals.

---

## 🛠 Tech Stack & Skills Demonstrated
| Area | Stack / Library | Highlights shown in this project |
|------|-----------------|-----------------------------------|
| UI | **.NET 8** WinForms | Responsive layout, DPI awareness |
| Parsing | Custom recursive-descent engine | Handles `+ - * / ^ √`, parentheses & unary ops |
| Numerics | `decimal`, `Math.Pow`, `Math.Sqrt` | Avoids floating-point drift, supports big numbers |
| Error Handling | C# exceptions | Detects divide-by-zero, malformed input |
| Architecture | SOLID principles | Separated engine vs. UI, easy to unit-test |
| Tooling | `dotnet` CLI | Cross-platform build & run |

## ✨ What the App Does
### Core Functionality
* Accepts free-form expressions like `(8-5)*2.5^3` or `√(2) + 1/3`.
* Supports arbitrary nesting, unary plus/minus, and right-associative exponentiation.
* Returns results as **decimal** for reliable financial or scientific precision.

### Visuals & UX
* Familiar calculator form with history pane for step-by-step evaluation *(placeholder form)*.
* Keyboard shortcuts for operators and Enter-to-evaluate.
* Graceful error dialogs instead of silent failures.

### Engineering Features
* Hand-rolled grammar (`Expression → Term {(+|-) Term}` etc.)—no external parser dependencies.
* Culture-invariant parsing (`InvariantCulture`) to avoid locale issues.
* Extensible engine (e.g., add trig or factorial) without touching UI code.

## 🏃 Running Locally
> **Prerequisites:** .NET 8 SDK (download from <https://dotnet.microsoft.com>)  
> *(WinForms ships in-box—no extra workloads needed)*

```bash
# clone & run
git clone https://github.com/RazorSDU/BigCalculatorApp.git
cd BigCalculatorApp
dotnet build
dotnet run --project BigCalculatorApp
