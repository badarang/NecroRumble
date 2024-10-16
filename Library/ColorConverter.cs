using UnityEngine;

public static class ColorConverter
{
    public static Color32 ConvertToColor32(int inputNumber)
    {
        // 입력 숫자를 RGBA로 변환
        byte r = (byte)((inputNumber >> 24) & 0xFF);
        byte g = (byte)((inputNumber >> 16) & 0xFF);
        byte b = (byte)((inputNumber >> 8) & 0xFF);
        byte a = (byte)(inputNumber & 0xFF);

        // Color32로 변환
        Color32 resultColor = new Color32(r, g, b, a);
        return resultColor;
    }
}