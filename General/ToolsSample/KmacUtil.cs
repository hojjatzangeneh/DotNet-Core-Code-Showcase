using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ToolsSample;
public static class KmacUtil
{
    /// <summary>
    /// محاسبه KMAC128 برای پیام داده شده با کلید مشخص.
    /// برگشتی: رشته هگز (lowercase).
    /// نیازمندی: net9.0+ و پلتفرمی که Kmac128 را پشتیبانی کند.
    /// </summary>
    /// <param name="keyUtf8">کلید به صورت رشته UTF-8 (در محیط واقعی از بایت های تصادفی استفاده کنید)</param>
    /// <param name="message">پیامی که می خواهید MAC گرفته شود</param>
    /// <param name="outputBytes">طول خروجی به بایت (مثلاً 32 برای 256 بیت)</param>
    /// <param name="customization">customization string اختیاری</param>
    /// <returns>hex string (lowercase)</returns>
    public static string ComputeKmac128(string keyUtf8 , string message , int outputBytes = 32)
    {
        if ( !Kmac128.IsSupported )
            throw new PlatformNotSupportedException("KMAC128 not supported on this platform.");

        byte[] key = Encoding.UTF8.GetBytes(keyUtf8);
        byte[] msg = Encoding.UTF8.GetBytes(message);

        // اگر می خواهی از customization استفاده کنی، باید آن را به صورت بایت بدهی.
        // متد HashData overload استاندارد اجازه ی customization را مستقیم نمی دهد در همه ی نسخه ها.
        // ساده ترین حالت: بدون customization از API استفاده کنیم:
        byte[] tag = Kmac128.HashData(key , msg , outputBytes);

        return ToHexLower(tag);
    }
    public static string ComputeKmac128(string keyUtf8 , string message , int outputBytes = 32 ,string? customizationUtf8 = null )
    {
        if ( !Kmac128.IsSupported )
            throw new PlatformNotSupportedException("KMAC128 not supported on this platform.");

        byte[] key = Encoding.UTF8.GetBytes(keyUtf8);
        byte[] msg = Encoding.UTF8.GetBytes(message);
        ReadOnlySpan<byte> custSpan = customizationUtf8 is null
            ? ReadOnlySpan<byte>.Empty
            : Encoding.UTF8.GetBytes(customizationUtf8);

        // استفاده از overload که customization را می پذیرد
        byte[] tag = Kmac128.HashData(key , msg , outputBytes , custSpan);

        return ToHexLower(tag);
    }
    static string ToHexLower(byte[] b)
    {
        var sb = new StringBuilder(b.Length * 2);
        foreach ( var bt in b )
            sb.Append(bt.ToString("x2"));
        return sb.ToString();
    }
}

