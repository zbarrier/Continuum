using System.Runtime.CompilerServices;

namespace Continuum.AsyncExtensions;

/// <summary>
/// Enables awaiting tuples of tasks, producing a tuple of their results.
/// </summary>
/// <remarks>
/// The tasks run concurrently and are awaited with <see cref="Task.WhenAll(Task[])"/>. If any task fails, awaiting the
/// tuple throws the first exception; inspect the individual tasks to observe every failure.
/// </remarks>
public static class TaskTupleExtensions
{
    /// <summary>
    /// Enables awaiting a tuple of two tasks, producing a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The result type of task 1.</typeparam>
    /// <typeparam name="T2">The result type of task 2.</typeparam>
    /// <param name="tasks">The tasks to await.</param>
    /// <returns>An awaiter for the combined results.</returns>
    public static TaskAwaiter<(T1, T2)> GetAwaiter<T1, T2>(this (Task<T1>, Task<T2>) tasks)
    {
        return WhenAll(tasks).GetAwaiter();

        static async Task<(T1, T2)> WhenAll((Task<T1>, Task<T2>) tasks)
        {
            var (task1, task2) = tasks;
            await Task.WhenAll(task1, task2).ConfigureAwait(false);
            return (task1.Result, task2.Result);
        }
    }

    /// <summary>
    /// Enables awaiting a tuple of three tasks, producing a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The result type of task 1.</typeparam>
    /// <typeparam name="T2">The result type of task 2.</typeparam>
    /// <typeparam name="T3">The result type of task 3.</typeparam>
    /// <param name="tasks">The tasks to await.</param>
    /// <returns>An awaiter for the combined results.</returns>
    public static TaskAwaiter<(T1, T2, T3)> GetAwaiter<T1, T2, T3>(this (Task<T1>, Task<T2>, Task<T3>) tasks)
    {
        return WhenAll(tasks).GetAwaiter();

        static async Task<(T1, T2, T3)> WhenAll((Task<T1>, Task<T2>, Task<T3>) tasks)
        {
            var (task1, task2, task3) = tasks;
            await Task.WhenAll(task1, task2, task3).ConfigureAwait(false);
            return (task1.Result, task2.Result, task3.Result);
        }
    }

    /// <summary>
    /// Enables awaiting a tuple of four tasks, producing a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The result type of task 1.</typeparam>
    /// <typeparam name="T2">The result type of task 2.</typeparam>
    /// <typeparam name="T3">The result type of task 3.</typeparam>
    /// <typeparam name="T4">The result type of task 4.</typeparam>
    /// <param name="tasks">The tasks to await.</param>
    /// <returns>An awaiter for the combined results.</returns>
    public static TaskAwaiter<(T1, T2, T3, T4)> GetAwaiter<T1, T2, T3, T4>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>) tasks)
    {
        return WhenAll(tasks).GetAwaiter();

        static async Task<(T1, T2, T3, T4)> WhenAll((Task<T1>, Task<T2>, Task<T3>, Task<T4>) tasks)
        {
            var (task1, task2, task3, task4) = tasks;
            await Task.WhenAll(task1, task2, task3, task4).ConfigureAwait(false);
            return (task1.Result, task2.Result, task3.Result, task4.Result);
        }
    }

    /// <summary>
    /// Enables awaiting a tuple of five tasks, producing a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The result type of task 1.</typeparam>
    /// <typeparam name="T2">The result type of task 2.</typeparam>
    /// <typeparam name="T3">The result type of task 3.</typeparam>
    /// <typeparam name="T4">The result type of task 4.</typeparam>
    /// <typeparam name="T5">The result type of task 5.</typeparam>
    /// <param name="tasks">The tasks to await.</param>
    /// <returns>An awaiter for the combined results.</returns>
    public static TaskAwaiter<(T1, T2, T3, T4, T5)> GetAwaiter<T1, T2, T3, T4, T5>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>) tasks)
    {
        return WhenAll(tasks).GetAwaiter();

        static async Task<(T1, T2, T3, T4, T5)> WhenAll((Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>) tasks)
        {
            var (task1, task2, task3, task4, task5) = tasks;
            await Task.WhenAll(task1, task2, task3, task4, task5).ConfigureAwait(false);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result);
        }
    }

    /// <summary>
    /// Enables awaiting a tuple of six tasks, producing a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The result type of task 1.</typeparam>
    /// <typeparam name="T2">The result type of task 2.</typeparam>
    /// <typeparam name="T3">The result type of task 3.</typeparam>
    /// <typeparam name="T4">The result type of task 4.</typeparam>
    /// <typeparam name="T5">The result type of task 5.</typeparam>
    /// <typeparam name="T6">The result type of task 6.</typeparam>
    /// <param name="tasks">The tasks to await.</param>
    /// <returns>An awaiter for the combined results.</returns>
    public static TaskAwaiter<(T1, T2, T3, T4, T5, T6)> GetAwaiter<T1, T2, T3, T4, T5, T6>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>) tasks)
    {
        return WhenAll(tasks).GetAwaiter();

        static async Task<(T1, T2, T3, T4, T5, T6)> WhenAll((Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>) tasks)
        {
            var (task1, task2, task3, task4, task5, task6) = tasks;
            await Task.WhenAll(task1, task2, task3, task4, task5, task6).ConfigureAwait(false);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result);
        }
    }

    /// <summary>
    /// Enables awaiting a tuple of seven tasks, producing a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The result type of task 1.</typeparam>
    /// <typeparam name="T2">The result type of task 2.</typeparam>
    /// <typeparam name="T3">The result type of task 3.</typeparam>
    /// <typeparam name="T4">The result type of task 4.</typeparam>
    /// <typeparam name="T5">The result type of task 5.</typeparam>
    /// <typeparam name="T6">The result type of task 6.</typeparam>
    /// <typeparam name="T7">The result type of task 7.</typeparam>
    /// <param name="tasks">The tasks to await.</param>
    /// <returns>An awaiter for the combined results.</returns>
    public static TaskAwaiter<(T1, T2, T3, T4, T5, T6, T7)> GetAwaiter<T1, T2, T3, T4, T5, T6, T7>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>) tasks)
    {
        return WhenAll(tasks).GetAwaiter();

        static async Task<(T1, T2, T3, T4, T5, T6, T7)> WhenAll((Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>) tasks)
        {
            var (task1, task2, task3, task4, task5, task6, task7) = tasks;
            await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7).ConfigureAwait(false);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result);
        }
    }

    /// <summary>
    /// Enables awaiting a tuple of eight tasks, producing a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The result type of task 1.</typeparam>
    /// <typeparam name="T2">The result type of task 2.</typeparam>
    /// <typeparam name="T3">The result type of task 3.</typeparam>
    /// <typeparam name="T4">The result type of task 4.</typeparam>
    /// <typeparam name="T5">The result type of task 5.</typeparam>
    /// <typeparam name="T6">The result type of task 6.</typeparam>
    /// <typeparam name="T7">The result type of task 7.</typeparam>
    /// <typeparam name="T8">The result type of task 8.</typeparam>
    /// <param name="tasks">The tasks to await.</param>
    /// <returns>An awaiter for the combined results.</returns>
    public static TaskAwaiter<(T1, T2, T3, T4, T5, T6, T7, T8)> GetAwaiter<T1, T2, T3, T4, T5, T6, T7, T8>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>, Task<T8>) tasks)
    {
        return WhenAll(tasks).GetAwaiter();

        static async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll((Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>, Task<T8>) tasks)
        {
            var (task1, task2, task3, task4, task5, task6, task7, task8) = tasks;
            await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8).ConfigureAwait(false);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result);
        }
    }

    /// <summary>
    /// Enables awaiting a tuple of nine tasks, producing a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The result type of task 1.</typeparam>
    /// <typeparam name="T2">The result type of task 2.</typeparam>
    /// <typeparam name="T3">The result type of task 3.</typeparam>
    /// <typeparam name="T4">The result type of task 4.</typeparam>
    /// <typeparam name="T5">The result type of task 5.</typeparam>
    /// <typeparam name="T6">The result type of task 6.</typeparam>
    /// <typeparam name="T7">The result type of task 7.</typeparam>
    /// <typeparam name="T8">The result type of task 8.</typeparam>
    /// <typeparam name="T9">The result type of task 9.</typeparam>
    /// <param name="tasks">The tasks to await.</param>
    /// <returns>An awaiter for the combined results.</returns>
    public static TaskAwaiter<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> GetAwaiter<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>, Task<T8>, Task<T9>) tasks)
    {
        return WhenAll(tasks).GetAwaiter();

        static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> WhenAll((Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>, Task<T8>, Task<T9>) tasks)
        {
            var (task1, task2, task3, task4, task5, task6, task7, task8, task9) = tasks;
            await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8, task9).ConfigureAwait(false);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result, task9.Result);
        }
    }

    /// <summary>
    /// Enables awaiting a tuple of ten tasks, producing a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The result type of task 1.</typeparam>
    /// <typeparam name="T2">The result type of task 2.</typeparam>
    /// <typeparam name="T3">The result type of task 3.</typeparam>
    /// <typeparam name="T4">The result type of task 4.</typeparam>
    /// <typeparam name="T5">The result type of task 5.</typeparam>
    /// <typeparam name="T6">The result type of task 6.</typeparam>
    /// <typeparam name="T7">The result type of task 7.</typeparam>
    /// <typeparam name="T8">The result type of task 8.</typeparam>
    /// <typeparam name="T9">The result type of task 9.</typeparam>
    /// <typeparam name="T10">The result type of task 10.</typeparam>
    /// <param name="tasks">The tasks to await.</param>
    /// <returns>An awaiter for the combined results.</returns>
    public static TaskAwaiter<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> GetAwaiter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>, Task<T8>, Task<T9>, Task<T10>) tasks)
    {
        return WhenAll(tasks).GetAwaiter();

        static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> WhenAll((Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>, Task<T8>, Task<T9>, Task<T10>) tasks)
        {
            var (task1, task2, task3, task4, task5, task6, task7, task8, task9, task10) = tasks;
            await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10).ConfigureAwait(false);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result, task9.Result, task10.Result);
        }
    }

    /// <summary>
    /// Enables awaiting a tuple of eleven tasks, producing a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The result type of task 1.</typeparam>
    /// <typeparam name="T2">The result type of task 2.</typeparam>
    /// <typeparam name="T3">The result type of task 3.</typeparam>
    /// <typeparam name="T4">The result type of task 4.</typeparam>
    /// <typeparam name="T5">The result type of task 5.</typeparam>
    /// <typeparam name="T6">The result type of task 6.</typeparam>
    /// <typeparam name="T7">The result type of task 7.</typeparam>
    /// <typeparam name="T8">The result type of task 8.</typeparam>
    /// <typeparam name="T9">The result type of task 9.</typeparam>
    /// <typeparam name="T10">The result type of task 10.</typeparam>
    /// <typeparam name="T11">The result type of task 11.</typeparam>
    /// <param name="tasks">The tasks to await.</param>
    /// <returns>An awaiter for the combined results.</returns>
    public static TaskAwaiter<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> GetAwaiter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>, Task<T8>, Task<T9>, Task<T10>, Task<T11>) tasks)
    {
        return WhenAll(tasks).GetAwaiter();

        static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> WhenAll((Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>, Task<T8>, Task<T9>, Task<T10>, Task<T11>) tasks)
        {
            var (task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11) = tasks;
            await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11).ConfigureAwait(false);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result, task9.Result, task10.Result, task11.Result);
        }
    }

    /// <summary>
    /// Enables awaiting a tuple of twelve tasks, producing a tuple of their results.
    /// </summary>
    /// <typeparam name="T1">The result type of task 1.</typeparam>
    /// <typeparam name="T2">The result type of task 2.</typeparam>
    /// <typeparam name="T3">The result type of task 3.</typeparam>
    /// <typeparam name="T4">The result type of task 4.</typeparam>
    /// <typeparam name="T5">The result type of task 5.</typeparam>
    /// <typeparam name="T6">The result type of task 6.</typeparam>
    /// <typeparam name="T7">The result type of task 7.</typeparam>
    /// <typeparam name="T8">The result type of task 8.</typeparam>
    /// <typeparam name="T9">The result type of task 9.</typeparam>
    /// <typeparam name="T10">The result type of task 10.</typeparam>
    /// <typeparam name="T11">The result type of task 11.</typeparam>
    /// <typeparam name="T12">The result type of task 12.</typeparam>
    /// <param name="tasks">The tasks to await.</param>
    /// <returns>An awaiter for the combined results.</returns>
    public static TaskAwaiter<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> GetAwaiter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>, Task<T8>, Task<T9>, Task<T10>, Task<T11>, Task<T12>) tasks)
    {
        return WhenAll(tasks).GetAwaiter();

        static async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> WhenAll((Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>, Task<T8>, Task<T9>, Task<T10>, Task<T11>, Task<T12>) tasks)
        {
            var (task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12) = tasks;
            await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10, task11, task12).ConfigureAwait(false);
            return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result, task9.Result, task10.Result, task11.Result, task12.Result);
        }
    }
}
