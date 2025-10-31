// File: Services/CharacterProcessRegistry.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MultiXIVLauncher.Services
{
    public sealed class LaunchedCharacterInfo
    {
        public string CharacterId { get; }
        public int ProcessId { get; }
        public string ProcessName { get; }
        public DateTime LaunchedAt { get; }

        internal LaunchedCharacterInfo(string characterId, Process proc)
        {
            CharacterId = characterId;
            ProcessId = proc.Id;
            ProcessName = SafeGetName(proc);
            LaunchedAt = DateTime.UtcNow;
        }

        private static string SafeGetName(Process p)
        {
            try { return p.ProcessName; } catch { return "unknown"; }
        }
    }

    /// <summary>
    /// Thread-safe registry of launched characters mapped to their running game process.
    /// </summary>
    public static class CharacterProcessRegistry
    {
        // Map characterId -> process
        private static readonly ConcurrentDictionary<string, Process> _byCharacterId =
            new ConcurrentDictionary<string, Process>(StringComparer.OrdinalIgnoreCase);

        // Map pid -> characterId (reverse lookup)
        private static readonly ConcurrentDictionary<int, string> _byPid =
            new ConcurrentDictionary<int, string>();

        /// <summary>
        /// Tracks a character's game process. Replaces any previous registration for this character.
        /// Automatically removes entries when the process exits.
        /// </summary>
        public static void Track(string characterId, Process proc)
        {
            if (string.IsNullOrWhiteSpace(characterId)) throw new ArgumentNullException(nameof(characterId));
            if (proc == null) throw new ArgumentNullException(nameof(proc));

            // Remove any stale entry for this character
            if (_byCharacterId.TryRemove(characterId, out var oldProc))
            {
                try { Unsubscribe(oldProc); } catch { /* ignore */ }
                _byPid.TryRemove(oldProc.Id, out _);
            }

            _byCharacterId[characterId] = proc;
            _byPid[proc.Id] = characterId;

            Subscribe(proc);
        }

        /// <summary>
        /// Returns snapshot list of all tracked characters with their PIDs.
        /// </summary>
        public static IReadOnlyList<LaunchedCharacterInfo> GetAll()
        {
            return _byCharacterId
                .Select(kv =>
                {
                    var characterId = kv.Key;
                    var proc = kv.Value;
                    if (proc == null) return null;
                    try
                    {
                        if (proc.HasExited) return null;
                        return new LaunchedCharacterInfo(characterId, proc);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(x => x != null)
                .ToList();
        }

        /// <summary>
        /// Try get process by characterId.
        /// </summary>
        public static bool TryGetProcess(string characterId, out Process process)
        {
            process = null;
            if (string.IsNullOrWhiteSpace(characterId)) return false;
            if (_byCharacterId.TryGetValue(characterId, out var p))
            {
                try
                {
                    if (!p.HasExited) { process = p; return true; }
                }
                catch { /* fall through */ }
            }
            // cleanup stale
            RemoveByCharacterId(characterId);
            return false;
        }

        /// <summary>
        /// Try get characterId for a given PID.
        /// </summary>
        public static bool TryGetCharacterId(int pid, out string characterId)
        {
            if (_byPid.TryGetValue(pid, out characterId))
            {
                if (_byCharacterId.TryGetValue(characterId, out var p))
                {
                    try { if (!p.HasExited) return true; } catch { }
                }
                // stale
                RemoveByPid(pid);
            }
            characterId = null;
            return false;
        }

        /// <summary>
        /// Manually remove an entry by characterId.
        /// </summary>
        public static void RemoveByCharacterId(string characterId)
        {
            if (_byCharacterId.TryRemove(characterId, out var proc))
            {
                try { Unsubscribe(proc); } catch { }
                _byPid.TryRemove(proc.Id, out _);
            }
        }

        /// <summary>
        /// Manually remove an entry by PID.
        /// </summary>
        public static void RemoveByPid(int pid)
        {
            if (_byPid.TryRemove(pid, out var charId))
            {
                _byCharacterId.TryRemove(charId, out var proc);
                if (proc != null) { try { Unsubscribe(proc); } catch { } }
            }
        }

        /// <summary>
        /// Clears the registry.
        /// </summary>
        public static void Clear()
        {
            foreach (var kv in _byCharacterId)
            {
                try { Unsubscribe(kv.Value); } catch { }
            }
            _byCharacterId.Clear();
            _byPid.Clear();
        }

        private static void Subscribe(Process proc)
        {
            try
            {
                proc.EnableRaisingEvents = true;
                proc.Exited += OnProcExited;
            }
            catch { /* ignore */ }
        }

        private static void Unsubscribe(Process proc)
        {
            try { proc.Exited -= OnProcExited; } catch { /* ignore */ }
        }

        private static void OnProcExited(object sender, EventArgs e)
        {
            var proc = sender as Process;
            if (proc == null) return;

            if (_byPid.TryRemove(proc.Id, out var charId))
            {
                _byCharacterId.TryRemove(charId, out _);
            }
        }
    }
}
