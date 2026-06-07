using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace scp714
{
    public class Handler : CustomEventsHandler
    {
        public override void OnServerRoundStarted() // Spawn System - state of CanSpawn
        {
            if (!SUB.Instance.Config.CanSpawn) // Controls the state of CanSpawn 
                return;

            Timing.CallDelayed(0.5f, () =>
            {
                RandomSpawn();
            });
        }

        private void RandomSpawn() // Spawn System - state of CanSpawn
        {
            List<RoomName> possibleRooms = new List<RoomName>
            {
                //LCZ ROOMS
                RoomName.LczGlassroom,
                RoomName.LczGreenhouse,
                RoomName.LczToilets,
                RoomName.Lcz173,
                RoomName.LczAirlock,
                //HCZ ROOMS
                RoomName.Hcz049,
                RoomName.Hcz106,
                RoomName.Hcz939,
                RoomName.HczServers,
                //EZ ROOMS
                RoomName.EzRedroom,
                RoomName.EzEvacShelter,
                RoomName.EzIntercom,
                RoomName.EzCollapsedTunnel
            };
            var scp714reason = new CustomReasonDamageHandler(
                "Died from exhaustion..", // Reason of death of Ragdoll
                -1f,
                "Died from exhaustion." // CASSIE, idk why it wants it
            );
            RoomName random = possibleRooms[UnityEngine.Random.Range(0, possibleRooms.Count)];
            Room getroom = Room.Get(random).FirstOrDefault();
            Vector3 spawnPos = getroom.Position + Vector3.up * 1.1f;
            Pickup ring = Pickup.Create(ItemType.Coin, spawnPos + Vector3.left * 0.3f); // position of SCP-714
            ring.Spawn();
            SUB.CustomItems.Add(ring.Serial);
            Ragdoll rag = Ragdoll.SpawnRagdoll(RoleTypeId.Scientist, spawnPos, Quaternion.identity, scp714reason, "???", null, null, null); // position of Ragdoll
            rag.Position = spawnPos;
        }
        public override void OnPlayerInteractingScp330(PlayerInteractingScp330EventArgs args) // SCP-714 protects you from SCP-330
        {
            Item ring = Get714(args.Player);
            if (args.Player == null) return;

            if (ring != null)
            {
                args.IsAllowed = false;
                args.Player.SendHint("Some mysterious force is stopping me from taking it...", 4f);
            }
        }

        private Item Get714(Player player) // check the state of InInventory, and returns inter value
        {
            if (player == null)
                return null;

            return player.Items.FirstOrDefault(i => i != null && SUB.CustomItems.Contains(i.Serial));
        }

        private bool scp714(Player player) // check the state of InInventory, and returns boolean value
        {
            Item ring = Get714(player);

            if (ring == null)
                return false;

            if (SUB.Instance.Config.InInventory)
                return true;

            return player.CurrentItem != null && player.CurrentItem.Serial == ring.Serial;
        }

        public override void OnPlayerHurting(PlayerHurtingEventArgs args) // SCP-049 can't attack while ring is worn
        {
            if (args.Player == null || args.Attacker == null || args.Attacker.Role != RoleTypeId.Scp049) return;
            Item ring = Get714(args.Player);

            if (ring == null)
                return;

            args.IsAllowed = false; // Doesn't need disable effect - SCP-049's attack = Effect
            int chance = UnityEngine.Random.Range(0, 101);
            if (chance <= 33) // 33% that SCP-049 takes off SCP-714 from player
            {
                args.Attacker.SendHitMarker(0);
                args.Player.DropItem(ring);
                args.Player.SendHint("SCP-049 has taken off SCP-714!", 5);
                args.Attacker.SendHint($"You have taken off {args.Player}' SCP-714!", 5);
            }
            else
            {
                args.Attacker.SendHint("Treatment doesn't work...");
            }
        }
            


        public override void OnPlayerUpdatedEffect(PlayerEffectUpdatedEventArgs args) // Prevents getting
        {
            bool hasRing = scp714(args.Player);

            if (!hasRing)
                return;

            if (args.Effect is AmnesiaVision ||
                args.Effect is Invigorated || args.Effect is Blurred ||
                args.Effect is Corroding || args.Effect is Deafened ||
                args.Effect is Traumatized || args.Effect is Sinkhole ||
                args.Effect is Scp207 || args.Effect is AntiScp207 ||
                args.Effect is Flashed || args.Effect is Scp1853) // list of effects to prevent
            {
                args.Player.DisableEffect<AmnesiaVision>();
                args.Player.DisableEffect<Invigorated>();
                args.Player.DisableEffect<Blurred>();
                args.Player.DisableEffect<Corroding>();
                args.Player.DisableEffect<Deafened>();
                args.Player.DisableEffect<Traumatized>();
                args.Player.DisableEffect<Sinkhole>();
                args.Player.DisableEffect<Scp207>();
                args.Player.DisableEffect<AntiScp207>();
                args.Player.DisableEffect<Flashed>();
                args.Player.DisableEffect<Scp1853>();
            }

        }

        public IEnumerator<float> RingCoroutine() // Timer && SCP-714 logic
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);

                foreach (Player player in Player.List)
                {
                    if (player == null || !player.IsAlive || player.Nickname == null)
                        continue;

                    try
                    {
                        bool hasRing = scp714(player);

                        if (hasRing)
                        {
                            player.StaminaRemaining = 0f; // Sets stamina to low
                            player.EnableEffect<Slowness>(15, -1, false); // Makes player "tired"


                            if (!SUB.RingDeathTimers.ContainsKey(player.PlayerId))
                            {
                                SUB.RingDeathTimers[player.PlayerId] = SUB.Instance.Config.TimeWear*1f; // Sets for timer a value
                            }

                            SUB.RingDeathTimers[player.PlayerId] -= 1f; // Reverse timer
                            float timeLeft = SUB.RingDeathTimers[player.PlayerId];
                            if (timeLeft <= 0f) // Timer has got end
                            {
                                player.Kill("Exhausted."); // Reason of death
                                SUB.RingDeathTimers.Remove(player.PlayerId); // Resets timer after death
                                continue;
                            }

                            player.SendHint(
                                $"<voffset=-4em><pos=-90%><color=yellow>Equipped: SCP-714</color></pos></voffset>" +
                                $"<voffset=-5em><pos=-90%><color=yellow>{timeLeft}</color></pos></voffset>",
                                1.1f
                            );
                        }
                        else
                        {
                            if (SUB.RingDeathTimers.ContainsKey(player.PlayerId))
                            {
                                SUB.RingDeathTimers.Remove(player.PlayerId); // Resets timer after drop
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LabApi.Features.Console.Logger.Error($"Error cicle {player.Nickname}: {ex}");
                    }
                }
            }
        }
    }
}
