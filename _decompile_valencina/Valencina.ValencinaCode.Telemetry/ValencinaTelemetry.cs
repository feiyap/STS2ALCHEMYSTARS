using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib;
using STS2RitsuLib.Telemetry;
using Valencina.ValencinaCode.Character;

namespace Valencina.ValencinaCode.Telemetry;

internal static class ValencinaTelemetry
{
	private const string EndpointEnvironmentVariable = "VALENCINA_TELEMETRY_ENDPOINT";

	private const string ProductionEndpoint = "https://valencina-telemetry.arabidopsis.workers.dev/v1/ingest";

	public static void Register()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		if (!TryValidateEndpoint(Environment.GetEnvironmentVariable("VALENCINA_TELEMETRY_ENDPOINT") ?? "https://valencina-telemetry.arabidopsis.workers.dev/v1/ingest", out string normalizedEndpoint))
		{
			MainFile.Logger.Info("[ValencinaTelemetry] Not registered: no valid HTTPS endpoint is configured. Set VALENCINA_TELEMETRY_ENDPOINT for local testing or fill ProductionEndpoint for release builds.", 1);
			return;
		}
		TelemetryApplicant val = new TelemetryApplicant();
		val.set_ApplicantId("Valencina");
		val.set_OwnerModId("Valencina");
		val.set_DisplayName("Valencina balance telemetry");
		val.set_Adapter((ITelemetryAdapter)new HttpJsonTelemetryAdapter(normalizedEndpoint, (IReadOnlyDictionary<string, string>)null));
		val.set_Requests((IReadOnlyList<TelemetryRequest>)new _003C_003Ez__ReadOnlySingleElementList<TelemetryRequest>(TelemetryRequest.RunHistory("Send the complete vanilla run-history record for runs containing Valencina, including characters, decks, relics, map history, ascension, run time, and result. This is used to evaluate card balance and future changes.", (IReadOnlyList<string>)null, (Func<RunEndedEvent, bool>)IsValencinaRun)));
		TelemetryApplicant val2 = val;
		RitsuLibFramework.RegisterTelemetryApplicant(val2);
		if (!RitsuLibFramework.GetTelemetryApplicants().Any((TelemetryApplicant candidate) => string.Equals(candidate.ApplicantId, "Valencina", StringComparison.Ordinal)))
		{
			MainFile.Logger.Error("[ValencinaTelemetry] RitsuLib did not retain the applicant after registration.", 1);
			return;
		}
		MainFile.Logger.Info($"[ValencinaTelemetry] Registration verified; requests={val2.Requests.Count}, endpoint={normalizedEndpoint}", 1);
	}

	private static bool IsValencinaRun(RunEndedEvent runEnded)
	{
		ModelId valencinaId = ModelDb.GetId<Valencina.ValencinaCode.Character.Valencina>();
		return ((RunEndedEvent)(ref runEnded)).Run.Players?.Any((SerializablePlayer player) => player.CharacterId == valencinaId) ?? false;
	}

	private static bool TryValidateEndpoint(string? endpoint, out string normalizedEndpoint)
	{
		normalizedEndpoint = string.Empty;
		if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri result))
		{
			return false;
		}
		bool num = result.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
		bool flag = result.IsLoopback && result.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase);
		if (!num && !flag)
		{
			return false;
		}
		normalizedEndpoint = result.AbsoluteUri;
		return true;
	}
}
