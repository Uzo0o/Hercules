using System.Collections.Generic;

namespace Hercules.Models.Templates;

/// <summary>
/// Everything needed to restore a full match setup without re-entering it by
/// hand: the FIBA connection details, every Dashboard stat->vMix mapping row,
/// every Script Trigger row, and every Overlay Automation row.
///
/// Deliberately does NOT store live vMix objects (VmixInput/VmixField) - their
/// "Key" is assigned by vMix at runtime and isn't guaranteed to be the same
/// the next time vMix is opened, even with the same preset loaded. Instead
/// each row stores the vMix input's Title and field Name (the stable, human-
/// chosen identifiers) and gets re-matched against whatever vMix reports
/// after "Refresh vMix Sources" - see MappingRowViewModel.TryResolvePendingVmixMatch.
/// </summary>
public class HerculesTemplate
{
    // Bumped only if the shape of this file changes in a way that needs
    // migration logic on load. Not enforced anywhere yet - just here so a
    // future version has something to check against.
    public string FormatVersion { get; set; } = "1";
    public string SavedAtUtc { get; set; } = string.Empty;

    public string FibaIpAddress { get; set; } = "127.0.0.1";
    public string FibaPort { get; set; } = "7677";

    public List<MappingRowTemplate> MappingRows { get; set; } = new();
    public List<ScriptTriggerRowTemplate> ScriptTriggerRows { get; set; } = new();
    public List<OverlayAutomationRowTemplate> OverlayAutomationRows { get; set; } = new();
}

// One Dashboard row: an optional prefix/suffix, which FIBA stat, and which
// vMix input/field it's routed to.
//
// FibaStatDisplayName (not the FibaStatKey enum) is the match key on purpose:
// FibaStatRegistry generates 12 entries sharing the same enum value for
// per-player roster stats (e.g. HomeRosterFoulsPersonal appears once per
// roster slot, 1 through 12) - the enum alone can't tell "Home Player 3 -
// Fouls" apart from "Home Player 7 - Fouls", but the DisplayName is unique
// across the whole registry.
public class MappingRowTemplate
{
    public string? FibaStatDisplayName { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
    public string? VmixInputTitle { get; set; }
    public string? VmixFieldName { get; set; }
}

// One Script Trigger row. FibaScriptTriggerKey entries ARE all unique (no
// per-slot duplication like the stat registry above), so the enum name
// itself is a safe match key here.
public class ScriptTriggerRowTemplate
{
    public string? TriggerKey { get; set; }
    public string ScriptName { get; set; } = string.Empty;
}

// One Overlay Automation row.
public class OverlayAutomationRowTemplate
{
    public string? TriggerKey { get; set; }
    public string? VmixInputTitle { get; set; }
    public int OverlayChannel { get; set; } = 1;
    public bool AutoHideEnabled { get; set; } = true;
    public string AutoHideMs { get; set; } = "3000";
}