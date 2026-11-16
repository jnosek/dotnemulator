using System.Runtime.CompilerServices;

namespace Dotnemulator.Abstraction.Hardware;

/// <summary>
/// Shared validation and mask calculation for fixed-width hardware values such as <see cref="Bus"/> and <see cref="Register"/>.
/// </summary>
public static class BitWidth
{
    /// <summary>
    /// Smallest supported width in bits.
    /// </summary>
    public const int MinSize = 1;

    /// <summary>
    /// Largest supported width in bits (values are stored in an <see cref="int"/>).
    /// </summary>
    public const int MaxSize = 32;

    /// <summary>
    /// Validates <paramref name="size"/> and returns a mask with the low <paramref name="size"/> bits set.
    /// </summary>
    /// <remarks>
    /// The mask is computed by shifting <see cref="uint.MaxValue"/> right instead of using <c>(1 &lt;&lt; size) - 1</c>.
    /// C# only uses the low 5 bits of a shift count, so <c>1 &lt;&lt; 32</c> is <c>1</c> and the naive form
    /// produces a mask of 0 for a 32-bit width.
    /// </remarks>
    /// <param name="size">Width in bits, from <see cref="MinSize"/> to <see cref="MaxSize"/>.</param>
    /// <param name="paramName">Name reported in the exception; filled in by the compiler.</param>
    /// <returns>The mask, e.g. <c>0xFF</c> for 8 bits or <c>-1</c> (all bits set) for 32 bits.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is outside the supported range.</exception>
    public static int GetMask(int size, [CallerArgumentExpression(nameof(size))] string? paramName = null)
    {
        if(size < MinSize)
            throw new ArgumentOutOfRangeException(paramName, size, $"Size must be greater than or equal to {MinSize}.");

        if(size > MaxSize)
            throw new ArgumentOutOfRangeException(paramName, size, $"Size must be less than or equal to {MaxSize}.");

        return (int)(uint.MaxValue >> (MaxSize - size));
    }
}
