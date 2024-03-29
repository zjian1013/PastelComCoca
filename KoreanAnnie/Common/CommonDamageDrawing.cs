﻿namespace KoreanAnnie.Common
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using Color = System.Drawing.Color;

    internal class CommonDamageDrawing
    {
        #region Constants

        private const int Height = 8;

        private const int Width = 103;

        #endregion

        #region Fields

        public bool Active = true;

        private readonly Color barColor = Color.Lime;

        private readonly ICommonChampion champion;

        private readonly Color comboDamageColor = Color.FromArgb(100, Color.Black);

        private readonly Render.Text text = new Render.Text(0, 0, "KILLABLE", 20, new ColorBGRA(255, 0, 0, 255));

        private DrawDamageDelegate amountOfDamage;

        #endregion

        #region Constructors and Destructors

        public CommonDamageDrawing(ICommonChampion champion)
        {
            this.champion = champion;
        }

        #endregion

        #region Delegates

        public delegate float DrawDamageDelegate(Obj_AI_Hero hero);

        #endregion

        #region Public Properties

        public DrawDamageDelegate AmountOfDamage
        {
            get
            {
                return amountOfDamage;
            }

            set
            {
                if (amountOfDamage == null)
                {
                    Drawing.OnDraw += DrawDamage;
                }
                amountOfDamage = value;
            }
        }

        #endregion

        #region Methods

        private void DrawDamage(EventArgs args)
        {
            if (!Enabled())
            {
                return;
            }

            foreach (
                Obj_AI_Hero champ in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(h => h.IsVisible && h.IsEnemy && h.IsValid && h.IsHPBarRendered))
            {
                float damage = amountOfDamage(champ);

                if (damage > 0)
                {
                    Vector2 pos = champ.HPBarPosition;

                    if (KoreanUtils.GetParamBool(champion.MainMenu, "killableindicator")
                        && (damage > champ.Health + 50f))
                    {
                        Render.Circle.DrawCircle(champ.Position, 100, Color.Red);
                        Render.Circle.DrawCircle(champ.Position, 75, Color.Red);
                        Render.Circle.DrawCircle(champ.Position, 50, Color.Red);
                        text.X = (int)pos.X + 40;
                        text.Y = (int)pos.Y - 20;
                        text.OnEndScene();
                    }

                    if (KoreanUtils.GetParamBool(champion.MainMenu, "damageindicator"))
                    {
                        float healthAfterDamage = Math.Max(0, champ.Health - damage) / champ.MaxHealth;
                        float posY = pos.Y + 20f;
                        float posDamageX = pos.X + 12f + Width * healthAfterDamage;
                        float posCurrHealthX = pos.X + 12f + Width * champ.Health / champ.MaxHealth;

                        Drawing.DrawLine(posDamageX, posY, posDamageX, posY + Height, 2, barColor);

                        float diff = (posCurrHealthX - posDamageX) + 3;

                        float pos1 = pos.X + 9 + (107 * healthAfterDamage);

                        for (int i = 0; i < diff; i++)
                        {
                            Drawing.DrawLine(pos1 + i, posY, pos1 + i, posY + Height, 1, comboDamageColor);
                        }
                    }
                }
            }
        }

        private bool Enabled()
        {
            return ((Active) && (amountOfDamage != null)
                    && ((KoreanUtils.GetParamBool(champion.MainMenu, "damageindicator"))
                        || KoreanUtils.GetParamBool(champion.MainMenu, "killableindicator")));
        }

        #endregion
    }
}