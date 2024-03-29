﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace SKORengar
{
   internal class Program
    {

        public const string ChampionName = "Rengar";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;

        public static Spell W;

        public static Spell E;

        public static Spell R;

        public static Menu Config;

        public static Items.Item HDR;

        public static Items.Item BKR;

        public static Items.Item TMT;

        public static Items.Item BWC;

        public static Items.Item YOU;

        public static Items.Item SOD;

        public static SpellSlot IgniteSlot;

        private static Obj_AI_Hero Player;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args) 
        {
            Player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != ChampionName) return;

            SKOUpdater.InitializeSKOUpdate();

            W = new Spell(SpellSlot.W, 450);
            E = new Spell(SpellSlot.E, 1000);
            R = new Spell(SpellSlot.R, 1100);

            E.SetSkillshot(0.250f, 70, 1500, true, SkillshotType.SkillshotLine);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            HDR = new Items.Item(3074, 175f);
            TMT = new Items.Item(3077, 175f);
            BKR = new Items.Item(3153, 450f);
            BWC = new Items.Item(3144, 450f);
            YOU = new Items.Item(3142, 185f);
            

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            //SKO Rengar
            Config = new Menu(ChampionName, "SKORengar", true);

            //TargetSelector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalker
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo
            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("AutoW", "Auto W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("Combo").AddItem(new MenuItem("TripleQ", "Triple Q").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            
            //Harass
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E")).SetValue(true);
            Config.SubMenu("Harass").AddItem(new MenuItem("ActiveHarass", "Harass key").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));

            //Farm
            Config.AddSubMenu(new Menu("Lane Clear", "Lane"));
            Config.SubMenu("Lane").AddItem(new MenuItem("UseQLane", "Use Q")).SetValue(true);
            Config.SubMenu("Lane").AddItem(new MenuItem("UseWLane", "Use W")).SetValue(true);
            Config.SubMenu("Lane").AddItem(new MenuItem("UseELane", "Use E")).SetValue(true);
            Config.SubMenu("Lane").AddItem(new MenuItem("ActiveLane", "Lane Key").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            
            //Kill Steal
            Config.AddSubMenu(new Menu("KillSteal", "Ks"));
            Config.SubMenu("Ks").AddItem(new MenuItem("ActiveKs", "Use KillSteal")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseQKs", "Use Q")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseWKs", "Use W")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseEKs", "Use E")).SetValue(true);
            Config.SubMenu("Ks").AddItem(new MenuItem("UseIgnite", "Use Ignite")).SetValue(true);


            //Drawings
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            Config.AddToMainMenu();

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;

           // Game.PrintChat("<font color='#1d87f2'>SKORengar Loaded!</font>");
        }

        private static void OnGameUpdate(EventArgs args) 
        {
           Player = ObjectManager.Player;
           Q = new Spell(SpellSlot.Q, Player.AttackRange + 50);
           SOD = new Items.Item(3131, Player.AttackRange + 50);

            Orbwalker.SetAttacks(true);
            if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active) 
            {
                Combo();
            }
            if (Config.Item("TripleQ").GetValue<KeyBind>().Active)
            {
                
                TripleQ();
                
                
            }
            if (Config.Item("ActiveHarass").GetValue<KeyBind>().Active)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                Harass();
            }
            if (Config.Item("ActiveLane").GetValue<KeyBind>().Active)
            {
                Farm();
            }
            if (Config.Item("ActiveKs").GetValue<bool>()) {
                KillSteal();
            }
            if (Config.Item("AutoW").GetValue<bool>())
            {
                Heal();

            }

        }

        private static void Combo() 
        {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            Orbwalker.SetAttacks((!W.IsReady() || E.IsReady()));

            if (target != null) 
            {
                //Foracity Combo
                if (Player.Distance(target) <= Q.Range && Player.Mana == 5) {
                    Q.Cast();
                }
                else if (Player.Distance(target) > Q.Range && Player.Distance(target) <= E.Range && Player.Mana == 5) 
                {
                    E.Cast(target);
                }

                //Normal Combo
                if (Player.Distance(target) <= Q.Range && Q.IsReady()) 
                {
                    Q.Cast();
                }
                if (Player.Distance(target) <= W.Range && W.IsReady())
                {
                    if (Player.Mana == 5) return;
                    W.Cast(target);
                }
                if (Player.Distance(target) <= E.Range && E.IsReady())
                {
                    E.Cast(target);
                }
                if (Config.Item("UseItems").GetValue<bool>()) {
                    BKR.Cast(target);
                    YOU.Cast();
                    BWC.Cast(target);
                    SOD.Cast();
                    if (Player.Distance(target) <= HDR.Range)
                    {
                        HDR.Cast(target);
                    }
                    if (Player.Distance(target) <= TMT.Range)
                    {
                        TMT.Cast(target);
                    }
                }
            }
        }

        private static void Harass()
        {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            Orbwalker.SetAttacks((!W.IsReady() || E.IsReady()));

            if (target != null)
            {
                //Ferocity Combo
                if (Player.Distance(target) > Q.Range && Player.Distance(target) <= E.Range && Player.Mana == 5)
                {
                    E.Cast(target);
                }

                //Normal Combo
                if (Player.Distance(target) <= W.Range && W.IsReady())
                {
                    W.Cast(target);
                }
                if (Player.Distance(target) <= E.Range && E.IsReady())
                {
                    E.Cast(target);
                }
            }
            if (Config.Item("UseItems").GetValue<bool>())
            {
                BKR.Cast(target);
                YOU.Cast();
                BWC.Cast(target);
                SOD.Cast();
                if (Player.Distance(target) <= HDR.Range)
                {
                    HDR.Cast(target);
                }
                if (Player.Distance(target) <= TMT.Range)
                {
                    TMT.Cast(target);
                }
            }
        }

        //Heal
        private static void Heal() {
        if (Player.Health <= (Player.MaxHealth * 20 / 100) && Player.Mana == 5)
                {
                    W.Cast();
         }
        }
        //TripleQ Combo
        private static void TripleQ() 
        {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);

            if (Player.Distance(target) <= R.Range && Q.IsReady() && R.IsReady() && Player.Mana == 5)
            {
                if (!Player.HasBuff("RengarR")) {
                    R.Cast();
                }
                Q1();
                Q2();
                Q3(target);
 
            }
            if (Config.Item("UseItems").GetValue<bool>())
            {
                BKR.Cast(target);
                YOU.Cast();
                BWC.Cast(target);
                SOD.Cast();
                if (Player.Distance(target) <= HDR.Range)
                {
                    HDR.Cast(target);
                }
                if (Player.Distance(target) <= TMT.Range)
                {
                    TMT.Cast(target);
                }
            }
            
        }
        private static void Q1(){
        
            if (Player.HasBuff("RengarR")){
            Q.Cast();
        }
        
        }
        private static void Q2()
        {
            if (Q.IsReady() && Player.Mana >= 2) 
            {
                Q.Cast();
            }
            
        }
        private static void Q3(Obj_AI_Hero target)
        {
            if (Q.IsReady()) {
                Q.Cast();
               W.Cast(target);
                E.Cast(target);
            }

        }
        private static void KillSteal() {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            var igniteDmg = DamageLib.getDmg(target, DamageLib.SpellType.IGNITE);
            var Qdmg = DamageLib.getDmg(target, DamageLib.SpellType.Q);
            var Wdmg = DamageLib.getDmg(target, DamageLib.SpellType.W);
            var Edmg = DamageLib.getDmg(target, DamageLib.SpellType.E);
            if (target != null && Config.Item("UseIgnite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown &&
                    Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (igniteDmg > target.Health)
                {
                    Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
                }
            }

            if (Player.Distance(target) <= Q.Range && Q.IsReady() && target != null && Config.Item("UseQKs").GetValue<bool>()) 
            {
                if (Qdmg >= target.Health) {
                    Q.Cast(target);
                    Orbwalker.SetAttacks(true);
                }
            }
            if (Player.Distance(target) <= W.Range && E.IsReady() && target != null && Config.Item("UseWKs").GetValue<bool>())
            {
                if (Wdmg >= target.Health)
                {
                    W.Cast(target);
                }
            }
            if (Player.Distance(target) <= E.Range && E.IsReady() && target != null && Config.Item("UseEKs").GetValue<bool>())
            {
                if (Edmg >= target.Health)
                {
                    E.CastOnUnit(target);
                }
            }

        }

        private static void Farm() {
            if (!Orbwalking.CanMove(40)) return;
            var Minions = MinionManager.GetMinions(Player.ServerPosition, W.Range);

            if (Config.Item("UseQLane").GetValue<bool>() && Q.IsReady()) {

                foreach (var minion in Minions) {
                    if (minion.IsValidTarget() && Player.Distance(minion) <= Q.Range) {

                        Q.Cast();
                        return;
                    }               
                }
            }
            else if (Config.Item("UseWLane").GetValue<bool>() && W.IsReady())
            {

                foreach (var minion in Minions)
                {
                    if (minion.IsValidTarget() && Player.Distance(minion) <= W.Range)
                    {

                        W.Cast(minion);
                        return;
                    }
                }
            }else if(Config.Item("UseELane").GetValue<bool>() && E.IsReady())
            {

                foreach (var minion in Minions)
                {
                    if (minion.IsValidTarget() && Player.Distance(minion) <= E.Range)
                    {

                        E.Cast(minion);
                        return;
                    }
                }
            }else if (Config.Item("UseItems").GetValue<bool>())
            {
                foreach (var minion in Minions)
                {
                    if (Player.Distance(minion) <= HDR.Range)
                    {
                        HDR.Cast(minion);
                    }
                    if (Player.Distance(minion) <= TMT.Range)
                    {
                        TMT.Cast(minion);
                    }
                }
            }
        }

        

        private static void OnDraw(EventArgs args) 
        {
            if (Config.Item("CircleLag").GetValue<bool>())
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value,
                        Config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Config.Item("DrawW").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value,
                        Config.Item("CircleQuality").GetValue<Slider>().Value);
                }
                if (Config.Item("DrawE").GetValue<bool>())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value,
                        Config.Item("CircleQuality").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White);
                }
                if (Config.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White);
                }
                if (Config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White);
                }

            }
        }
    }
}
