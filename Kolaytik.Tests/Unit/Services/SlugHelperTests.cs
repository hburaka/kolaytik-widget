using FluentAssertions;
using Kolaytik.Infrastructure.Helpers;

namespace Kolaytik.Tests.Unit.Services;

public class SlugHelperTests
{
    // ── Turkish character normalization ───────────────────────────────────────

    [Theory]
    [InlineData("Merhaba Dünya",          "merhaba-dunya")]
    [InlineData("İstanbul Şubesi",         "istanbul-subesi")]
    [InlineData("Üniversite Öğrencisi",    "universite-ogrencisi")]
    [InlineData("Çalışanlar Listesi",      "calisanlar-listesi")]
    [InlineData("Doktor Görevlendirme",    "doktor-gorevlendirme")]
    [InlineData("BÜYÜK HARF İŞLEM",        "buyuk-harf-islem")]
    public void ToSlug_ConvertsturkishCharacters(string input, string expected)
        => SlugHelper.ToSlug(input).Should().Be(expected);

    // ── Spaces and dashes ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("Hello World",        "hello-world")]
    [InlineData("  leading trailing  ", "leading-trailing")]
    [InlineData("multiple   spaces",  "multiple-spaces")]
    [InlineData("foo---bar",          "foo-bar")]
    [InlineData("a - b - c",          "a-b-c")]
    public void ToSlug_NormalizesSpacesAndDashes(string input, string expected)
        => SlugHelper.ToSlug(input).Should().Be(expected);

    // ── Special character removal ─────────────────────────────────────────────

    [Theory]
    [InlineData("Şube #1 (Merkez)", "sube-1-merkez")]
    [InlineData("100% Kalite!",     "100-kalite")]
    [InlineData("A & B",            "a-b")]
    [InlineData("foo@bar.com",      "foobarcom")]
    public void ToSlug_RemovesSpecialCharacters(string input, string expected)
        => SlugHelper.ToSlug(input).Should().Be(expected);

    // ── Length truncation ─────────────────────────────────────────────────────

    [Fact]
    public void ToSlug_TruncatesAt80Characters()
    {
        var input  = new string('a', 100);
        var result = SlugHelper.ToSlug(input);
        result.Length.Should().BeLessOrEqualTo(80);
    }

    [Fact]
    public void ToSlug_DoesNotEndWithDash_WhenTruncated()
    {
        // 80 a's followed by a space and "b" — dash would be at position 80 after truncation
        var input  = new string('a', 80) + " b";
        var result = SlugHelper.ToSlug(input);
        result.Should().NotEndWith("-");
    }

    [Fact]
    public void ToSlug_ExactlyAt80Chars_IsNotTruncated()
    {
        var input  = new string('a', 80);
        var result = SlugHelper.ToSlug(input);
        result.Length.Should().Be(80);
    }

    // ── Edge cases ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("!!!")]
    [InlineData("---")]
    [InlineData("   ")]
    public void ToSlug_ReturnsEmpty_WhenNoAlphanumericContent(string input)
    {
        var result = SlugHelper.ToSlug(input);
        result.Should().NotStartWith("-").And.NotEndWith("-");
    }

    [Fact]
    public void ToSlug_LowercasesAllOutput()
    {
        SlugHelper.ToSlug("HELLO WORLD").Should().Be("hello-world");
    }

    [Fact]
    public void ToSlug_PreservesDigits()
    {
        SlugHelper.ToSlug("Depo 3B").Should().Be("depo-3b");
    }
}
