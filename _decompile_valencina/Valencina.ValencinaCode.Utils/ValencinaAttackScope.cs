using System;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Valencina.ValencinaCode.Utils;

public static class ValencinaAttackScope
{
	private sealed class Frame(Creature owner, bool preserveBreathingMethod, decimal breathingMethodDamageMultiplier, Frame? parent)
	{
		public Creature Owner { get; } = owner;

		public bool PreserveBreathingMethod { get; } = preserveBreathingMethod;

		public decimal BreathingMethodDamageMultiplier { get; } = breathingMethodDamageMultiplier;

		public Frame? Parent { get; } = parent;
	}

	private sealed class Scope(Frame frame) : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				if (Current.Value == frame)
				{
					Current.Value = frame.Parent;
				}
			}
		}
	}

	private static readonly AsyncLocal<Frame?> Current = new AsyncLocal<Frame>();

	public static bool IsActive => Current.Value != null;

	public static bool ShouldSuppressBreathingMethodAfterAttack(Creature? owner)
	{
		for (Frame frame = Current.Value; frame != null; frame = frame.Parent)
		{
			if (frame.Owner == owner)
			{
				return frame.PreserveBreathingMethod;
			}
		}
		return false;
	}

	public static bool ShouldPreserveBreathingMethod(Creature? owner)
	{
		for (Frame frame = Current.Value; frame != null; frame = frame.Parent)
		{
			if (frame.Owner == owner)
			{
				return frame.PreserveBreathingMethod;
			}
		}
		return false;
	}

	public static decimal BreathingMethodDamageMultiplier(Creature? owner)
	{
		for (Frame frame = Current.Value; frame != null; frame = frame.Parent)
		{
			if (frame.Owner == owner)
			{
				return frame.BreathingMethodDamageMultiplier;
			}
		}
		return 1m;
	}

	public static IDisposable Enter(Creature owner, bool preserveBreathingMethod)
	{
		return Enter(owner, preserveBreathingMethod, 1m);
	}

	public static IDisposable Enter(Creature owner, bool preserveBreathingMethod, decimal breathingMethodDamageMultiplier)
	{
		Frame frame = new Frame(owner, preserveBreathingMethod, Math.Max(0m, breathingMethodDamageMultiplier), Current.Value);
		Current.Value = frame;
		return new Scope(frame);
	}

	public static async Task RunAsync(Creature? owner, bool preserveBreathingMethod, Func<Task> action)
	{
		await RunAsync(owner, preserveBreathingMethod, 1m, action);
	}

	public static async Task RunAsync(Creature? owner, bool preserveBreathingMethod, decimal breathingMethodDamageMultiplier, Func<Task> action)
	{
		if (owner == null)
		{
			await action();
			return;
		}
		using (Enter(owner, preserveBreathingMethod, breathingMethodDamageMultiplier))
		{
			await action();
		}
	}
}
