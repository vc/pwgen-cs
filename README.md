# pwgen-cs
Porting jbernard/pwgen code to C#

## License

This project is a C# port of [pwgen](https://github.com/jbernard/pwgen).  
The original project is licensed under the GNU General Public License, version 2 (GPL-2.0).  

This port is distributed under the terms of the **GNU General Public License v2.0 or later**.  
You should have received a copy of the GNU General Public License along with this program.  
If not, see <https://www.gnu.org/licenses/>.

## Usage Example

```csharp
using Pwgen;

var pw = new PhoneticPasswordGenerator();

int flag = PasswordFlags.Uppers | PasswordFlags.Digits;
var pass = pw.GeneratePhoneticPassword(16, flag);
```