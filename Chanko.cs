﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using Newtonsoft.Json;
using TreeSharp;
using System.Windows.Forms;
using System.Windows.Media;

namespace ChankoPlugin
{
    public class Chanko : BotPlugin
    {
        private Composite _coroutine;
        private ChankoSettings _settingsForm;
        private static int _buff = 48;   

        public override string Author
        {
            get { return "evo89"; }
        }

        public override string Name
        {
            get { return "Chanko"; }
        }
        
        public override Version Version
        {
            get { return new Version(0, 0, 1, 0); }
        }

        private static async Task<bool> EatFood()
        {
            if (Settings.Instance.FoodName == null || Settings.Instance.FoodName.Length == 0)
            {
                Logging.Write(Colors.Aquamarine, "[Chanko] No food selected, check your settings");
                return false;
            }

            var item = InventoryManager.FilledSlots.GetFoodItems().First(s => s.TrueItemId == Settings.Instance.Id);

            if (item == null) return false;

            if (FishingManager.State != FishingState.None)
            {
                Logging.Write(Colors.Aquamarine, "[Chanko] Stop fishing");
                Actionmanager.DoAction("Quit", Core.Me);
                await Coroutine.Wait(5000, () => FishingManager.State == FishingState.None);
            }

            if (Core.Me.IsMounted)
            {
                Logging.Write(Colors.Aquamarine, "[Chanko] Dismounting to eat");
                await CommonTasks.StopAndDismount();
            }

            Logging.Write(Colors.Aquamarine, "[Chanko] Eating " + item.EnglishName);
            item.UseItem();
            await Coroutine.Wait(5000, () => Core.Me.HasAura(_buff));

            return true;
        }

        public override void OnInitialize()
        {
            _coroutine = new Decorator(c => !Core.Me.HasAura(_buff), new ActionRunCoroutine(r => EatFood()));
        }

        public override void OnEnabled()
        {
            TreeRoot.OnStart += OnBotStart;
            TreeRoot.OnStop += OnBotStop;
            TreeHooks.Instance.OnHooksCleared += OnHooksCleared;

            if (TreeRoot.IsRunning)
            {
                AddHooks();
            }
        }

        public override void OnDisabled()
        {
            TreeRoot.OnStart += OnBotStart;
            TreeRoot.OnStop += OnBotStop;
            RemoveHooks();
            
        }

        public override void OnShutdown()
        {
            TreeRoot.OnStart -= OnBotStart;
            TreeRoot.OnStop -= OnBotStop;
            RemoveHooks();
        }

        public override bool WantButton
        {
            get { return true; }
        }

        public override void OnButtonPress()
        {
            if (_settingsForm == null || _settingsForm.IsDisposed || _settingsForm.Disposing)
            {
                _settingsForm = new ChankoSettings();
            }

            _settingsForm.ShowDialog();
        }

        private void AddHooks()
        {
            Logging.Write("Adding Chanko Hook");
            TreeHooks.Instance.InsertHook("TreeStart", 0, _coroutine);
        }

        private void RemoveHooks()
        {
            Logging.Write("Removing Chanko Hook");
            TreeHooks.Instance.RemoveHook("TreeStart", _coroutine);
        }

        private void OnBotStop(BotBase bot)
        {
            RemoveHooks();
        }

        private void OnBotStart(BotBase bot)
        {
            AddHooks();
        }

        private void OnHooksCleared(object sender, EventArgs e)
        {
            AddHooks();
        }
    }

    public class Settings : JsonSettings
    {
        [JsonIgnore]
        private static Settings _instance;

        public static Settings Instance
        {
            get { return _instance ?? (_instance = new Settings()); ; }
        }

        public Settings()
            : base(Path.Combine(CharacterSettingsDirectory, "Chanko.json"))
        {
        }

        [Setting]
        public string FoodName { get; set; }

        [Setting]
        public uint Id { get; set; }
    }
}
