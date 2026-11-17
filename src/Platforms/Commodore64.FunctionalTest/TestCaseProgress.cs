namespace Commodore64.FunctionalTest;

/// <summary>
/// Follows the functional test's current test case number and prints a line to the console for each test case
/// that passes.
/// </summary>
/// <remarks>
/// The ROM keeps the current test case number in <c>test_case</c> (<c>$0200</c>). It stores <c>0</c> at start, then
/// at the end of each test (the <c>next_test</c> macro) checks the number and stores the next one, and after the
/// last test stores <see cref="FunctionalTestSymbols.Io.AllTestsComplete"/>. So each write of a new number means
/// the test case before it passed. Feed this from a write-only memory watch: the ROM also reads the number.
/// </remarks>
internal sealed class TestCaseProgress(int allTestsComplete)
{
    private int? _current;

    /// <summary>
    /// Number of test cases that have passed.
    /// </summary>
    public int PassedCount { get; private set; }

    /// <summary>
    /// The test case running now, or <c>null</c> before the first one starts and after the last one has passed.
    /// </summary>
    public int? Current => _current;

    /// <summary>
    /// <c>true</c> once the ROM has marked every test case as complete.
    /// </summary>
    public bool IsComplete { get; private set; }

    /// <summary>
    /// Receives a value written to <c>test_case</c>.
    /// </summary>
    public void Write(int value)
    {
        // 0 is written by the start code; it does not end a test
        if(_current is int previous && value != 0)
        {
            PassedCount++;
            Console.WriteLine($"Test case {previous:X2} passed");
        }

        IsComplete = value == allTestsComplete;
        _current = IsComplete ? null : value;
    }
}
