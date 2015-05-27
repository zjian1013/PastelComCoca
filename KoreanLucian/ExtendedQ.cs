﻿namespace KoreanLucian
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using KoreanCommon;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    internal static class ExtendedQ
    {
        static private readonly Spell AdvancedQ = new Spell(SpellSlot.Q, 1100);

        static private readonly Func<Obj_AI_Hero, Obj_AI_Base, bool> CheckDistance =
            (champ, minion) =>
            Math.Abs(
                champ.Distance(ObjectManager.Player) - (minion.Distance(ObjectManager.Player) + minion.Distance(champ)))
            <= 3;

        static private readonly Func<Vector3, Vector3, Vector3, bool> CheckLine =
            (v1, v2, v3) =>
            Math.Abs((v1.X * v2.Y) + (v1.Y * v3.X) + (v2.X * v3.Y) - (v1.Y * v2.X) - (v1.X * v3.Y) - (v2.Y * v3.X))
            <= 25000;

        static private bool CheckHaras(CommonChampion lucian, Obj_AI_Hero target)
        {
            return KoreanUtils.GetParamBool(lucian.MainMenu, target.ChampionName.ToLowerInvariant());
        }

        static private bool ExtendedQIsReady(CommonChampion lucian, bool laneclear = false)
        {
            Spell q = lucian.Spells.Q;

            if (!KoreanUtils.GetParamBool(lucian.MainMenu, "extendedq") || !q.IsReady())
            {
                return false;
            }

            List<Obj_AI_Base> minions = MinionManager.GetMinions(AdvancedQ.Range);

            if (minions.Count == 0)
            {
                return false;
            }

            if (!laneclear)
            {
                if (lucian.Player.CountEnemiesInRange(AdvancedQ.Range) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        static public bool CastExtendedQ(this CommonChampion lucian)
        {
            if (!ExtendedQIsReady(lucian))
            {
                return false;
            }

            Spell q = lucian.Spells.Q;

            AdvancedQ.SetSkillshot(0.55f, 75f, float.MaxValue, false, SkillshotType.SkillshotLine);

            foreach (Obj_AI_Hero target in lucian.Player.GetEnemiesInRange(AdvancedQ.Range))
            {
                if (!CheckHaras(lucian, target))
                {
                    continue;
                }

                List<Vector2> position = new List<Vector2> { target.Position.To2D() };

                Obj_AI_Base colisionMinion =
                    AdvancedQ.GetCollision(lucian.Player.Position.To2D(), position)
                        .FirstOrDefault(
                            minion =>
                            q.CanCast(minion) && q.IsInRange(minion)
                            && CheckLine(lucian.Player.Position, minion.Position, target.ServerPosition)
                            && CheckDistance(target, minion)
                            && target.Distance(lucian.Player) > minion.Distance(lucian.Player));

                if (colisionMinion != null)
                {
                    return q.CastOnUnit(colisionMinion);
                }
            }

            return false;
        }

        public static bool CastExtendedQToLaneClear(this CommonChampion lucian)
        {
            if (!ExtendedQIsReady(lucian, true) || !lucian.Spells.Q.UseOnLaneClear)
            {
                return false;
            }

            AdvancedQ.SetSkillshot(0.55f, 75f, float.MaxValue, false, SkillshotType.SkillshotLine);

            List<Obj_AI_Base> minionsBase = MinionManager.GetMinions(
                    lucian.Player.Position,
                    AdvancedQ.Range,
                    MinionTypes.All,
                    MinionTeam.NotAlly,
                    MinionOrderTypes.MaxHealth);

            if (minionsBase.Count == 0)
            {
                return false;
            }

            Spell q = lucian.Spells.Q;
            
            foreach (Obj_AI_Base minion in minionsBase.Where(x => q.IsInRange(x)))
            {
                if (AdvancedQ.CountHits(minionsBase, minion.Position) >= KoreanUtils.GetParamSlider(lucian.MainMenu, "qcounthit"))
                {
                    q.CastOnUnit(minion);
                    Orbwalking.ResetAutoAttackTimer();
                    return true;
                }
            }

            return false;
        }
    }
}