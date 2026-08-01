using System;
using System.Threading.Tasks;

namespace Valencina.ValencinaCode.Utils;

public static class InstantAttackScope
{
	public static bool IsActive => InstantAttackState.IsActive;

	public static IDisposable Enter()
	{
		return InstantAttackState.Enter();
	}

	public static async Task RunAsync(Func<Task> action)
	{
		using (Enter())
		{
			await action();
		}
	}

	public static Task RunAsync(object? _, Func<Task> action)
	{
		return RunAsync(action);
	}

	public static async Task<T> RunAsync<T>(Func<Task<T>> action)
	{
		using (Enter())
		{
			return await action();
		}
	}

	public static Task<T> RunAsync<T>(object? _, Func<Task<T>> action)
	{
		return RunAsync(action);
	}
}
