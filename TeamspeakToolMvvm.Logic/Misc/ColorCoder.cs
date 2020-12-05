using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Misc {
    public static class ColorCoder {

        public static string ColorText(Color color, object text) {
            return $"[color=#{color.AsHex}]{text}[/color]";
        }

        public static string ColorPivotPercent(double val, double pivot, string valStr=null) {
            valStr = valStr ?? $"{val:0.##}%";
            return val < 0 ? ColorCoder.ErrorBright(valStr) : val > pivot ? ColorCoder.SuccessDim(valStr) : $"{valStr}";
        }
        public static string ColorPivot(int val, int pivot) {
            return val < pivot ? ColorCoder.ErrorBright(val) : val > pivot ? ColorCoder.SuccessDim(val) : $"{val}";
        }

        public static string ErrorBright(object text) {
            return $"[color=#{Color.LightRed.AsHex}]{text}[/color]";
        }
        public static string Error(object text) {
            return $"[color=#{Color.Red.AsHex}]{text}[/color]";
        }
        public static string ErrorDim(object text) {
            return $"[color=#{Color.DarkRed.AsHex}]{text}[/color]";
        }
        
        public static string Success(object text) {
            return $"[color=#{Color.Green.AsHex}]{text}[/color]";
        }
        public static string SuccessDim(object text) {
            return $"[color=#{Color.DarkGreen.AsHex}]{text}[/color]";
        }

        
        public static string Attention(object text) {
            return $"[color=#{Color.LightBlue.AsHex}]{text}[/color]";
        }

        public static string Username(object name) {
            return $"[b]@'{name}'[/b]";
        }
        public static string Bold(object text) {
            return $"[b]{text}[/b]";
        }
        public static string Currency(object text, string unit) {
            return $"[b]{text} {unit}[/b]";
        }

    }

    public class Color {
        

        public static Color DarkRed = new Color() { AsHex = "aa0000" };
        public static Color Red = new Color() { AsHex = "ff0000" };
        public static Color LightRed = new Color() { AsHex = "ff5555" };

        public static Color DarkGreen = new Color() { AsHex = "00aa00" };
        public static Color Green = new Color() { AsHex = "00ff00" };
        public static Color LightGreen = new Color() { AsHex = "55ff55" };

        public static Color DarkBlue = new Color() { AsHex = "0000aa" };
        public static Color Blue = new Color() { AsHex = "0000ff" };
        public static Color LightBlue = new Color() { AsHex = "5555ff" };

        public static Color Yellow = new Color() { AsHex = "bbbb00" };

        public string AsHex { get; set; }

        public static Color FromHex(string hex) {
            return new Color() { AsHex = hex };
        }
        public static Color FromRgb(int r, int g, int b) {
            return null;
        }
    }
}
