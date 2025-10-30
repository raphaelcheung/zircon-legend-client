using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Controls;
using Client.Envir;
using Client.Scenes;
using Client.Scenes.Views;
using Library;
using Library.SystemModels;
using SlimDX;
using S = Library.Network.ServerPackets;

namespace Client.Models
{
    public sealed class MonsterObject : MapObject
    {
        public override ObjectType Race => ObjectType.Monster;
        public override bool Blocking => base.Blocking && CompanionObject == null;

        public MonsterInfo MonsterInfo;
        
        public MirLibrary BodyLibrary;
        public int BodyOffSet = 1000;
        public int BodyShape;
        public int BodyFrame => DrawFrame + (BodyShape % 10) * BodyOffSet;

        public SoundIndex AttackSound, StruckSound, DieSound;

        public bool Extra, EasterEvent, ChristmasEvent, HalloweenEvent;

        public override int RenderY
        {
            get
            {
                int offset = 0;

                if (Image == MonsterImage.LobsterLord)
                    offset += 5;

                return base.RenderY + offset;
            }
        }

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(PetOwner))
                    return base.Name;

                return base.Name + $" ({PetOwner})";
            }
            set { base.Name = value; }
        }

        public ClientCompanionObject CompanionObject;

    // When a pet is assigned to the player this records the time it was summoned/assigned
        public DateTime SummonedTime { get; set; } = DateTime.MinValue;


        public MonsterImage Image;

        public MonsterObject(CompanionInfo info)
        {
            MonsterInfo = info.MonsterInfo;

            Stats = new Stats(MonsterInfo.Stats);

            Light = Stats[Stat.Light];

            Name = MonsterInfo.MonsterName;
            
            Direction = MirDirection.DownLeft;
            
            UpdateLibraries();

            SetAnimation(new ObjectAction(MirAction.Standing, Direction, Point.Empty));
        }
        public MonsterObject(S.ObjectMonster info)
        {
            ObjectID = info.ObjectID;

            MonsterInfo = Globals.MonsterInfoList.Binding.First(x => x.Index == info.MonsterIndex);

            CompanionObject = info.CompanionObject;

            Stats = new Stats(MonsterInfo.Stats);

            Light = Stats[Stat.Light];

            Name = CompanionObject?.Name ?? MonsterInfo.MonsterName;

            PetOwner = info.PetOwner;
            // Mark the summoned time for player's pets so we can avoid immediately buffing them
            if (!string.IsNullOrEmpty(PetOwner) && GameScene.Game?.User != null && PetOwner == GameScene.Game.User.Name)
                SummonedTime = CEnvir.Now;
            NameColour = info.NameColour;
            Extra = info.Extra;
            
            CurrentLocation = info.Location;
            Direction = info.Direction;
            
            Dead = info.Dead;
            Skeleton = info.Skeleton;

            EasterEvent = info.EasterEvent;
            HalloweenEvent = info.HalloweenEvent;
            ChristmasEvent = info.ChristmasEvent;

            Poison = info.Poison;

            foreach (BuffType type in info.Buffs)
                VisibleBuffs.Add(type);

            UpdateLibraries();

            SetFrame(new ObjectAction(!Dead ? MirAction.Standing : MirAction.Dead, MirDirection.Up, CurrentLocation));
            
            GameScene.Game.MapControl.AddObject(this);
            
            UpdateQuests();
        }
        public void UpdateLibraries()
        {
            BodyLibrary = null;

            Frames = new Dictionary<MirAnimation, Frame>(FrameSet.DefaultMonster);

            BodyOffSet = 1000;

            AttackSound = SoundIndex.None;
            StruckSound = SoundIndex.None;
            DieSound = SoundIndex.None;
            //OtherSounds

            Image = MonsterInfo.Image;


            #region new

            switch (this.Image)
            {
                case MonsterImage.Guard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out this.BodyLibrary);
                    this.BodyShape = 6;
                    break;
                case MonsterImage.Chicken:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.ChickenAttack;
                    this.StruckSound = SoundIndex.ChickenStruck;
                    this.DieSound = SoundIndex.ChickenDie;
                    break;
                case MonsterImage.Pig:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_12, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.PigAttack;
                    this.StruckSound = SoundIndex.PigStruck;
                    this.DieSound = SoundIndex.PigDie;
                    break;
                case MonsterImage.Deer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.DeerAttack;
                    this.StruckSound = SoundIndex.DeerStruck;
                    this.DieSound = SoundIndex.DeerDie;
                    break;
                case MonsterImage.Cow:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_13, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.CowAttack;
                    this.StruckSound = SoundIndex.CowStruck;
                    this.DieSound = SoundIndex.CowDie;
                    break;
                case MonsterImage.Sheep:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.SheepAttack;
                    this.StruckSound = SoundIndex.SheepStruck;
                    this.DieSound = SoundIndex.SheepDie;
                    break;
                case MonsterImage.ClawCat:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.ClawCatAttack;
                    this.StruckSound = SoundIndex.ClawCatStruck;
                    this.DieSound = SoundIndex.ClawCatDie;
                    break;
                case MonsterImage.Wolf:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.WolfAttack;
                    this.StruckSound = SoundIndex.WolfStruck;
                    this.DieSound = SoundIndex.WolfDie;
                    break;
                case MonsterImage.ForestYeti:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.ForestYetiAttack;
                    this.StruckSound = SoundIndex.ForestYetiStruck;
                    this.DieSound = SoundIndex.ForestYetiDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.ForestYeti.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.ChestnutTree:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_13, out this.BodyLibrary);
                    this.BodyShape = 7;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.ChestnutTree.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.CarnivorousPlant:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.CarnivorousPlantAttack;
                    this.StruckSound = SoundIndex.CarnivorousPlantStruck;
                    this.DieSound = SoundIndex.CarnivorousPlantDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.CarnivorousPlant.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Oma:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.OmaAttack;
                    this.StruckSound = SoundIndex.OmaStruck;
                    this.DieSound = SoundIndex.OmaDie;
                    break;
                case MonsterImage.TigerSnake:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.TigerSnakeAttack;
                    this.StruckSound = SoundIndex.TigerSnakeStruck;
                    this.DieSound = SoundIndex.TigerSnakeDie;
                    break;
                case MonsterImage.SpittingSpider:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.SpittingSpiderAttack;
                    this.StruckSound = SoundIndex.SpittingSpiderStruck;
                    this.DieSound = SoundIndex.SpittingSpiderDie;
                    break;
                case MonsterImage.Scarecrow:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.ScarecrowAttack;
                    this.StruckSound = SoundIndex.ScarecrowStruck;
                    this.DieSound = SoundIndex.ScarecrowDie;
                    break;
                case MonsterImage.OmaHero:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.OmaHeroAttack;
                    this.StruckSound = SoundIndex.OmaHeroStruck;
                    this.DieSound = SoundIndex.OmaHeroDie;
                    break;
                case MonsterImage.CaveBat:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.CaveBatAttack;
                    this.StruckSound = SoundIndex.CaveBatStruck;
                    this.DieSound = SoundIndex.CaveBatDie;
                    break;
                case MonsterImage.Scorpion:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.ScorpionAttack;
                    this.StruckSound = SoundIndex.ScorpionStruck;
                    this.DieSound = SoundIndex.ScorpionDie;
                    break;
                case MonsterImage.Skeleton:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.SkeletonAttack;
                    this.StruckSound = SoundIndex.SkeletonStruck;
                    this.DieSound = SoundIndex.SkeletonDie;
                    break;
                case MonsterImage.SkeletonAxeMan:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.SkeletonAxeManAttack;
                    this.StruckSound = SoundIndex.SkeletonAxeManStruck;
                    this.DieSound = SoundIndex.SkeletonAxeManDie;
                    break;
                case MonsterImage.SkeletonAxeThrower:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.SkeletonAxeThrowerAttack;
                    this.StruckSound = SoundIndex.SkeletonAxeThrowerStruck;
                    this.DieSound = SoundIndex.SkeletonAxeThrowerDie;
                    break;
                case MonsterImage.SkeletonWarrior:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.SkeletonWarriorAttack;
                    this.StruckSound = SoundIndex.SkeletonWarriorStruck;
                    this.DieSound = SoundIndex.SkeletonWarriorDie;
                    break;
                case MonsterImage.SkeletonLord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.AttackSound = SoundIndex.SkeletonLordAttack;
                    this.StruckSound = SoundIndex.SkeletonLordStruck;
                    this.DieSound = SoundIndex.SkeletonLordDie;
                    break;
                case MonsterImage.CaveMaggot:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_4, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.CaveMaggotAttack;
                    this.StruckSound = SoundIndex.CaveMaggotStruck;
                    this.DieSound = SoundIndex.CaveMaggotDie;
                    break;
                case MonsterImage.GhostSorcerer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.GhostSorcererAttack;
                    this.StruckSound = SoundIndex.GhostSorcererStruck;
                    this.DieSound = SoundIndex.GhostSorcererDie;
                    break;
                case MonsterImage.GhostMage:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.GhostMageAttack;
                    this.StruckSound = SoundIndex.GhostMageStruck;
                    this.DieSound = SoundIndex.GhostMageDie;
                    break;
                case MonsterImage.VoraciousGhost:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.VoraciousGhostAttack;
                    this.StruckSound = SoundIndex.VoraciousGhostStruck;
                    this.DieSound = SoundIndex.VoraciousGhostDie;
                    break;
                case MonsterImage.DevouringGhost:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.VoraciousGhostAttack;
                    this.StruckSound = SoundIndex.VoraciousGhostStruck;
                    this.DieSound = SoundIndex.VoraciousGhostDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.DevouringGhost.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.CorpseRaisingGhost:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.VoraciousGhostAttack;
                    this.StruckSound = SoundIndex.VoraciousGhostStruck;
                    this.DieSound = SoundIndex.VoraciousGhostDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.DevouringGhost.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.GhoulChampion:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.GhoulChampionAttack;
                    this.StruckSound = SoundIndex.GhoulChampionStruck;
                    this.DieSound = SoundIndex.GhoulChampionDie;
                    break;
                case MonsterImage.ArmoredAnt:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.ArmoredAntAttack;
                    this.StruckSound = SoundIndex.ArmoredAntStruck;
                    this.DieSound = SoundIndex.ArmoredAntDie;
                    break;
                case MonsterImage.AntSoldier:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_2, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.ArmoredAntAttack;
                    this.StruckSound = SoundIndex.ArmoredAntStruck;
                    this.DieSound = SoundIndex.ArmoredAntDie;
                    break;
                case MonsterImage.AntHealer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.ArmoredAntAttack;
                    this.StruckSound = SoundIndex.ArmoredAntStruck;
                    this.DieSound = SoundIndex.ArmoredAntDie;
                    break;
                case MonsterImage.AntNeedler:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.AttackSound = SoundIndex.AntNeedlerAttack;
                    this.StruckSound = SoundIndex.AntNeedlerStruck;
                    this.DieSound = SoundIndex.AntNeedlerDie;
                    break;
                case MonsterImage.ShellNipper:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.ShellNipperAttack;
                    this.StruckSound = SoundIndex.ShellNipperStruck;
                    this.DieSound = SoundIndex.ShellNipperDie;
                    break;
                case MonsterImage.Beetle:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.KeratoidAttack;
                    this.StruckSound = SoundIndex.KeratoidStruck;
                    this.DieSound = SoundIndex.KeratoidDie;
                    break;
                case MonsterImage.VisceralWorm:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.VisceralWormAttack;
                    this.StruckSound = SoundIndex.VisceralWormStruck;
                    this.DieSound = SoundIndex.VisceralWormDie;
                    break;
                case MonsterImage.MutantFlea:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.MutantFleaAttack;
                    this.StruckSound = SoundIndex.MutantFleaStruck;
                    this.DieSound = SoundIndex.MutantFleaDie;
                    break;
                case MonsterImage.PoisonousMutantFlea:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.PoisonousMutantFleaAttack;
                    this.StruckSound = SoundIndex.PoisonousMutantFleaStruck;
                    this.DieSound = SoundIndex.PoisonousMutantFleaDie;
                    break;
                case MonsterImage.BlasterMutantFlea:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.BlasterMutantFleaAttack;
                    this.StruckSound = SoundIndex.BlasterMutantFleaStruck;
                    this.DieSound = SoundIndex.BlasterMutantFleaDie;
                    break;
                case MonsterImage.WasHatchling:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.WasHatchlingAttack;
                    this.StruckSound = SoundIndex.WasHatchlingStruck;
                    this.DieSound = SoundIndex.WasHatchlingDie;
                    break;
                case MonsterImage.Centipede:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.AttackSound = SoundIndex.CentipedeAttack;
                    this.StruckSound = SoundIndex.CentipedeStruck;
                    this.DieSound = SoundIndex.CentipedeDie;
                    break;
                case MonsterImage.ButterflyWorm:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.ButterflyWormAttack;
                    this.StruckSound = SoundIndex.ButterflyWormStruck;
                    this.DieSound = SoundIndex.ButterflyWormDie;
                    break;
                case MonsterImage.MutantMaggot:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.MutantMaggotAttack;
                    this.StruckSound = SoundIndex.MutantMaggotStruck;
                    this.DieSound = SoundIndex.MutantMaggotDie;
                    break;
                case MonsterImage.Earwig:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.EarwigAttack;
                    this.StruckSound = SoundIndex.EarwigStruck;
                    this.DieSound = SoundIndex.EarwigDie;
                    break;
                case MonsterImage.IronLance:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.IronLanceAttack;
                    this.StruckSound = SoundIndex.IronLanceStruck;
                    this.DieSound = SoundIndex.IronLanceDie;
                    break;
                case MonsterImage.LordNiJae:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.LordNiJaeAttack;
                    this.StruckSound = SoundIndex.LordNiJaeStruck;
                    this.DieSound = SoundIndex.LordNiJaeDie;
                    break;
                case MonsterImage.RottingGhoul:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.RottingGhoulAttack;
                    this.StruckSound = SoundIndex.RottingGhoulStruck;
                    this.DieSound = SoundIndex.RottingGhoulDie;
                    break;
                case MonsterImage.DecayingGhoul:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.DecayingGhoulAttack;
                    this.StruckSound = SoundIndex.DecayingGhoulStruck;
                    this.DieSound = SoundIndex.DecayingGhoulDie;
                    break;
                case MonsterImage.BloodThirstyGhoul:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.BloodThirstyGhoulAttack;
                    this.StruckSound = SoundIndex.BloodThirstyGhoulStruck;
                    this.DieSound = SoundIndex.BloodThirstyGhoulDie;
                    break;
                case MonsterImage.SpinedDarkLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.AttackSound = SoundIndex.SpinedDarkLizardAttack;
                    this.StruckSound = SoundIndex.SpinedDarkLizardStruck;
                    this.DieSound = SoundIndex.SpinedDarkLizardDie;
                    break;
                case MonsterImage.UmaInfidel:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.UmaInfidelAttack;
                    this.StruckSound = SoundIndex.UmaInfidelStruck;
                    this.DieSound = SoundIndex.UmaInfidelDie;
                    break;
                case MonsterImage.UmaFlameThrower:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.UmaFlameThrowerAttack;
                    this.StruckSound = SoundIndex.UmaFlameThrowerStruck;
                    this.DieSound = SoundIndex.UmaFlameThrowerDie;
                    break;
                case MonsterImage.UmaAnguisher:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.UmaAnguisherAttack;
                    this.StruckSound = SoundIndex.UmaAnguisherStruck;
                    this.DieSound = SoundIndex.UmaAnguisherDie;
                    break;
                case MonsterImage.UmaKing:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_5, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.UmaKingAttack;
                    this.StruckSound = SoundIndex.UmaKingStruck;
                    this.DieSound = SoundIndex.UmaKingDie;
                    break;
                case MonsterImage.SpiderBat:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.SpiderBatAttack;
                    this.StruckSound = SoundIndex.SpiderBatStruck;
                    this.DieSound = SoundIndex.SpiderBatDie;
                    break;
                case MonsterImage.ArachnidGazer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.StruckSound = SoundIndex.ArachnidGazerStruck;
                    this.DieSound = SoundIndex.ArachnidGazerDie;
                    break;
                case MonsterImage.Larva:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.LarvaAttack;
                    this.StruckSound = SoundIndex.LarvaStruck;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.Larva.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.RedMoonGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.RedMoonGuardianAttack;
                    this.StruckSound = SoundIndex.RedMoonGuardianStruck;
                    this.DieSound = SoundIndex.RedMoonGuardianDie;
                    break;
                case MonsterImage.RedMoonProtector:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.RedMoonProtectorAttack;
                    this.StruckSound = SoundIndex.RedMoonProtectorStruck;
                    this.DieSound = SoundIndex.RedMoonProtectorDie;
                    break;
                case MonsterImage.VenomousArachnid:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_12, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.VenomousArachnidAttack;
                    this.StruckSound = SoundIndex.VenomousArachnidStruck;
                    this.DieSound = SoundIndex.VenomousArachnidDie;
                    break;
                case MonsterImage.DarkArachnid:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_12, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.DarkArachnidAttack;
                    this.StruckSound = SoundIndex.DarkArachnidStruck;
                    this.DieSound = SoundIndex.DarkArachnidDie;
                    break;
                case MonsterImage.RedMoonTheFallen:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.RedMoonTheFallenAttack;
                    this.StruckSound = SoundIndex.RedMoonTheFallenStruck;
                    this.DieSound = SoundIndex.RedMoonTheFallenDie;
                    break;
                case MonsterImage.ZumaSharpShooter:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.ZumaSharpShooterAttack;
                    this.StruckSound = SoundIndex.ZumaSharpShooterStruck;
                    this.DieSound = SoundIndex.ZumaSharpShooterDie;
                    break;
                case MonsterImage.ZumaFanatic:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.ZumaFanaticAttack;
                    this.StruckSound = SoundIndex.ZumaFanaticStruck;
                    this.DieSound = SoundIndex.ZumaFanaticDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.ZumaGuardian.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.ZumaGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.ZumaGuardianAttack;
                    this.StruckSound = SoundIndex.ZumaGuardianStruck;
                    this.DieSound = SoundIndex.ZumaGuardianDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.ZumaGuardian.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.ViciousRat:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.ViciousRatAttack;
                    this.StruckSound = SoundIndex.ViciousRatStruck;
                    this.DieSound = SoundIndex.ViciousRatDie;
                    break;
                case MonsterImage.ZumaKing:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.ZumaKingAttack;
                    this.StruckSound = SoundIndex.ZumaKingStruck;
                    this.DieSound = SoundIndex.ZumaKingDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.ZumaKing.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.EvilFanatic:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.EvilFanaticAttack;
                    this.StruckSound = SoundIndex.EvilFanaticStruck;
                    this.DieSound = SoundIndex.EvilFanaticDie;
                    break;
                case MonsterImage.Monkey:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.MonkeyAttack;
                    this.StruckSound = SoundIndex.MonkeyStruck;
                    this.DieSound = SoundIndex.MonkeyDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.Monkey.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.EvilElephant:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.EvilElephantAttack;
                    this.StruckSound = SoundIndex.EvilElephantStruck;
                    this.DieSound = SoundIndex.EvilElephantDie;
                    break;
                case MonsterImage.CannibalFanatic:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.AttackSound = SoundIndex.CannibalFanaticAttack;
                    this.StruckSound = SoundIndex.CannibalFanaticStruck;
                    this.DieSound = SoundIndex.CannibalFanaticDie;
                    break;
                case MonsterImage.SpikedBeetle:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_7, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.SpikedBeetleAttack;
                    this.StruckSound = SoundIndex.SpikedBeetleStruck;
                    this.DieSound = SoundIndex.SpikedBeetleDie;
                    break;
                case MonsterImage.NumaGrunt:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_13, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.NumaGruntAttack;
                    this.StruckSound = SoundIndex.NumaGruntStruck;
                    this.DieSound = SoundIndex.NumaGruntDie;
                    break;
                case MonsterImage.NumaMage:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_2, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.NumaMageAttack;
                    this.StruckSound = SoundIndex.NumaMageStruck;
                    this.DieSound = SoundIndex.NumaMageDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.NumaMage.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.NumaElite:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_2, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.NumaEliteAttack;
                    this.StruckSound = SoundIndex.NumaEliteStruck;
                    this.DieSound = SoundIndex.NumaEliteDie;
                    break;
                case MonsterImage.SandShark:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.SandSharkAttack;
                    this.StruckSound = SoundIndex.SandSharkStruck;
                    this.DieSound = SoundIndex.SandSharkDie;
                    break;
                case MonsterImage.StoneGolem:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.StoneGolemAttack;
                    this.StruckSound = SoundIndex.StoneGolemStruck;
                    this.DieSound = SoundIndex.StoneGolemDie;
                    break;
                case MonsterImage.WindfurySorceress:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.WindfurySorceressAttack;
                    this.StruckSound = SoundIndex.WindfurySorceressStruck;
                    this.DieSound = SoundIndex.WindfurySorceressDie;
                    break;
                case MonsterImage.CursedCactus:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.CursedCactusAttack;
                    this.StruckSound = SoundIndex.CursedCactusStruck;
                    this.DieSound = SoundIndex.CursedCactusDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.CursedCactus.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.NetherWorldGate:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out this.BodyLibrary);
                    this.BodyShape = 5;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.NetherWorldGate.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.RagingLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.RagingLizardAttack;
                    this.StruckSound = SoundIndex.RagingLizardStruck;
                    this.DieSound = SoundIndex.RagingLizardDie;
                    break;
                case MonsterImage.SawToothLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.SawToothLizardAttack;
                    this.StruckSound = SoundIndex.SawToothLizardStruck;
                    this.DieSound = SoundIndex.SawToothLizardDie;
                    break;
                case MonsterImage.MutantLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.MutantLizardAttack;
                    this.StruckSound = SoundIndex.MutantLizardStruck;
                    this.DieSound = SoundIndex.MutantLizardDie;
                    break;
                case MonsterImage.VenomSpitter:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.VenomSpitterAttack;
                    this.StruckSound = SoundIndex.VenomSpitterStruck;
                    this.DieSound = SoundIndex.VenomSpitterDie;
                    break;
                case MonsterImage.SonicLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.SonicLizardAttack;
                    this.StruckSound = SoundIndex.SonicLizardStruck;
                    this.DieSound = SoundIndex.SonicLizardDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.WestDesertLizard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.GiantLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.AttackSound = SoundIndex.GiantLizardAttack;
                    this.StruckSound = SoundIndex.GiantLizardStruck;
                    this.DieSound = SoundIndex.GiantLizardDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.WestDesertLizard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.CrazedLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.CrazedLizardAttack;
                    this.StruckSound = SoundIndex.CrazedLizardStruck;
                    this.DieSound = SoundIndex.CrazedLizardDie;
                    break;
                case MonsterImage.TaintedTerror:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.TaintedTerrorAttack;
                    this.StruckSound = SoundIndex.TaintedTerrorStruck;
                    this.DieSound = SoundIndex.TaintedTerrorDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.WestDesertLizard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.DeathLordJichon:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.DeathLordJichonAttack;
                    this.StruckSound = SoundIndex.DeathLordJichonStruck;
                    this.DieSound = SoundIndex.DeathLordJichonDie;
                    break;
                case MonsterImage.Minotaur:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.MinotaurAttack;
                    this.StruckSound = SoundIndex.MinotaurStruck;
                    this.DieSound = SoundIndex.MinotaurDie;
                    break;
                case MonsterImage.FrostMinotaur:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.FrostMinotaurAttack;
                    this.StruckSound = SoundIndex.FrostMinotaurStruck;
                    this.DieSound = SoundIndex.FrostMinotaurDie;
                    break;
                case MonsterImage.ShockMinotaur:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.FrostMinotaurAttack;
                    this.StruckSound = SoundIndex.FrostMinotaurStruck;
                    this.DieSound = SoundIndex.FrostMinotaurDie;
                    break;
                case MonsterImage.FlameMinotaur:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.AttackSound = SoundIndex.FrostMinotaurAttack;
                    this.StruckSound = SoundIndex.FrostMinotaurStruck;
                    this.DieSound = SoundIndex.FrostMinotaurDie;
                    break;
                case MonsterImage.FuryMinotaur:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.FrostMinotaurAttack;
                    this.StruckSound = SoundIndex.FrostMinotaurStruck;
                    this.DieSound = SoundIndex.FrostMinotaurDie;
                    break;
                case MonsterImage.BanyaLeftGuard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.BanyaLeftGuardAttack;
                    this.StruckSound = SoundIndex.BanyaLeftGuardStruck;
                    this.DieSound = SoundIndex.BanyaLeftGuardDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BanyaGuard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.BanyaRightGuard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.BanyaLeftGuardAttack;
                    this.StruckSound = SoundIndex.BanyaLeftGuardStruck;
                    this.DieSound = SoundIndex.BanyaLeftGuardDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BanyaGuard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.EmperorSaWoo:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_14, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.EmperorSaWooAttack;
                    this.StruckSound = SoundIndex.EmperorSaWooStruck;
                    this.DieSound = SoundIndex.EmperorSaWooDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.EmperorSaWoo.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.BoneArcher:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.BoneArcherAttack;
                    this.StruckSound = SoundIndex.BoneArcherStruck;
                    this.DieSound = SoundIndex.BoneArcherDie;
                    break;
                case MonsterImage.BoneBladesman:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.BoneArcherAttack;
                    this.StruckSound = SoundIndex.BoneArcherStruck;
                    this.DieSound = SoundIndex.BoneArcherDie;
                    break;
                case MonsterImage.BoneCaptain:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.BoneCaptainAttack;
                    this.StruckSound = SoundIndex.BoneCaptainStruck;
                    this.DieSound = SoundIndex.BoneCaptainDie;
                    break;
                case MonsterImage.BoneSoldier:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.BoneArcherAttack;
                    this.StruckSound = SoundIndex.BoneArcherStruck;
                    this.DieSound = SoundIndex.BoneArcherDie;
                    break;
                case MonsterImage.ArchLichTaedu:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_15, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.ArchLichTaeduAttack;
                    this.StruckSound = SoundIndex.ArchLichTaeduStruck;
                    this.DieSound = SoundIndex.ArchLichTaeduDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.ArchLichTaeda.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.WedgeMothLarva:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.WedgeMothLarvaAttack;
                    this.StruckSound = SoundIndex.WedgeMothLarvaStruck;
                    this.DieSound = SoundIndex.WedgeMothLarvaDie;
                    break;
                case MonsterImage.LesserWedgeMoth:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.LesserWedgeMothAttack;
                    this.StruckSound = SoundIndex.LesserWedgeMothStruck;
                    this.DieSound = SoundIndex.LesserWedgeMothDie;
                    break;
                case MonsterImage.WedgeMoth:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.WedgeMothAttack;
                    this.StruckSound = SoundIndex.WedgeMothStruck;
                    this.DieSound = SoundIndex.WedgeMothDie;
                    break;
                case MonsterImage.RedBoar:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.AttackSound = SoundIndex.RedBoarAttack;
                    this.StruckSound = SoundIndex.RedBoarStruck;
                    this.DieSound = SoundIndex.RedBoarDie;
                    break;
                case MonsterImage.ClawSerpent:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.ClawSerpentAttack;
                    this.StruckSound = SoundIndex.ClawSerpentStruck;
                    this.DieSound = SoundIndex.ClawSerpentDie;
                    break;
                case MonsterImage.BlackBoar:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.BlackBoarAttack;
                    this.StruckSound = SoundIndex.BlackBoarStruck;
                    this.DieSound = SoundIndex.BlackBoarDie;
                    break;
                case MonsterImage.TuskLord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_8, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.TuskLordAttack;
                    this.StruckSound = SoundIndex.TuskLordStruck;
                    this.DieSound = SoundIndex.TuskLordDie;
                    break;
                case MonsterImage.RazorTusk:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.RazorTuskAttack;
                    this.StruckSound = SoundIndex.RazorTuskStruck;
                    this.DieSound = SoundIndex.RazorTuskDie;
                    break;
                case MonsterImage.PinkGoddess:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_17, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.PinkGoddessAttack;
                    this.StruckSound = SoundIndex.PinkGoddessStruck;
                    this.DieSound = SoundIndex.PinkGoddessDie;
                    break;
                case MonsterImage.GreenGoddess:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_17, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.GreenGoddessAttack;
                    this.StruckSound = SoundIndex.GreenGoddessStruck;
                    this.DieSound = SoundIndex.GreenGoddessDie;
                    break;
                case MonsterImage.MutantCaptain:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_17, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.MutantCaptainAttack;
                    this.StruckSound = SoundIndex.MutantCaptainStruck;
                    this.DieSound = SoundIndex.MutantCaptainDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.WestDesertLizard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.StoneGriffin:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_17, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.StoneGriffinAttack;
                    this.StruckSound = SoundIndex.StoneGriffinStruck;
                    this.DieSound = SoundIndex.StoneGriffinDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BanyaGuard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.FlameGriffin:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.FlameGriffinAttack;
                    this.StruckSound = SoundIndex.FlameGriffinStruck;
                    this.DieSound = SoundIndex.FlameGriffinDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BanyaGuard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.WhiteBone:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_6, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.AttackSound = SoundIndex.WhiteBoneAttack;
                    this.StruckSound = SoundIndex.WhiteBoneStruck;
                    this.DieSound = SoundIndex.WhiteBoneDie;
                    break;
                case MonsterImage.Shinsu:
                    if (this.Extra)
                    {
                        CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out this.BodyLibrary);
                        this.BodyShape = 0;
                        this.AttackSound = SoundIndex.ShinsuBigAttack;
                        this.StruckSound = SoundIndex.ShinsuBigStruck;
                        this.DieSound = SoundIndex.ShinsuBigDie;
                        break;
                    }
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.None;
                    this.StruckSound = SoundIndex.ShinsuSmallStruck;
                    this.DieSound = SoundIndex.ShinsuSmallDie;
                    break;
                case MonsterImage.InfernalSoldier:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_26, out this.BodyLibrary);
                    this.BodyShape = 2;
                    break;
                case MonsterImage.CorpseStalker:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_2, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.CorpseStalkerAttack;
                    this.StruckSound = SoundIndex.CorpseStalkerStruck;
                    this.DieSound = SoundIndex.CorpseStalkerDie;
                    break;
                case MonsterImage.LightArmedSoldier:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.AttackSound = SoundIndex.LightArmedSoldierAttack;
                    this.StruckSound = SoundIndex.LightArmedSoldierStruck;
                    this.DieSound = SoundIndex.LightArmedSoldierDie;
                    break;
                case MonsterImage.CorrosivePoisonSpitter:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.CorrosivePoisonSpitterAttack;
                    this.StruckSound = SoundIndex.CorrosivePoisonSpitterStruck;
                    this.DieSound = SoundIndex.CorrosivePoisonSpitterDie;
                    break;
                case MonsterImage.PhantomSoldier:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.PhantomSoldierAttack;
                    this.StruckSound = SoundIndex.PhantomSoldierStruck;
                    this.DieSound = SoundIndex.PhantomSoldierDie;
                    break;
                case MonsterImage.MutatedOctopus:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.MutatedOctopusAttack;
                    this.StruckSound = SoundIndex.MutatedOctopusStruck;
                    this.DieSound = SoundIndex.MutatedOctopusDie;
                    break;
                case MonsterImage.AquaLizard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_10, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.AquaLizardAttack;
                    this.StruckSound = SoundIndex.AquaLizardStruck;
                    this.DieSound = SoundIndex.AquaLizardDie;
                    break;
                case MonsterImage.Stomper:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.AquaLizardAttack;
                    this.StruckSound = SoundIndex.AquaLizardStruck;
                    this.DieSound = SoundIndex.AquaLizardDie;
                    break;
                case MonsterImage.CrimsonNecromancer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_2, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.CrimsonNecromancerAttack;
                    this.StruckSound = SoundIndex.CrimsonNecromancerStruck;
                    this.DieSound = SoundIndex.CrimsonNecromancerDie;
                    break;
                case MonsterImage.ChaosKnight:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_2, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.ChaosKnightAttack;
                    this.DieSound = SoundIndex.ChaosKnightDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BanyaGuard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.PachonTheChaosBringer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_13, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.PachontheChaosbringerAttack;
                    this.StruckSound = SoundIndex.PachontheChaosbringerStruck;
                    this.DieSound = SoundIndex.PachontheChaosbringerDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.PachonTheChaosBringer.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.NumaCavalry:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_19, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.NumaCavalryAttack;
                    this.StruckSound = SoundIndex.NumaCavalryStruck;
                    this.DieSound = SoundIndex.NumaCavalryDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BanyaGuard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.NumaHighMage:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_19, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.NumaHighMageAttack;
                    this.StruckSound = SoundIndex.NumaHighMageStruck;
                    this.DieSound = SoundIndex.NumaHighMageDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BanyaGuard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.NumaStoneThrower:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_19, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.NumaStoneThrowerAttack;
                    this.StruckSound = SoundIndex.NumaStoneThrowerStruck;
                    this.DieSound = SoundIndex.NumaStoneThrowerDie;
                    break;
                case MonsterImage.NumaRoyalGuard:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_19, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.NumaRoyalGuardAttack;
                    this.StruckSound = SoundIndex.NumaRoyalGuardStruck;
                    this.DieSound = SoundIndex.NumaRoyalGuardDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.EmperorSaWoo.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.NumaArmoredSoldier:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_19, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.NumaArmoredSoldierAttack;
                    this.StruckSound = SoundIndex.NumaArmoredSoldierStruck;
                    this.DieSound = SoundIndex.NumaArmoredSoldierDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BanyaGuard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.IcyRanger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.IcyRangerAttack;
                    this.StruckSound = SoundIndex.IcyRangerStruck;
                    this.DieSound = SoundIndex.IcyRangerDie;
                    break;
                case MonsterImage.IcyGoddess:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_18, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.IcyGoddessAttack;
                    this.StruckSound = SoundIndex.IcyGoddessStruck;
                    this.DieSound = SoundIndex.IcyGoddessDie;
                    break;
                case MonsterImage.IcySpiritWarrior:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.IcySpiritWarriorAttack;
                    this.StruckSound = SoundIndex.IcySpiritWarriorStruck;
                    this.DieSound = SoundIndex.IcySpiritWarriorDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.NumaMage.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.IcySpiritGeneral:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.IcySpiritWarriorAttack;
                    this.StruckSound = SoundIndex.IcySpiritWarriorStruck;
                    this.DieSound = SoundIndex.IcySpiritWarriorDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.IcySpiritGeneral.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.GhostKnight:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.GhostKnightAttack;
                    this.StruckSound = SoundIndex.GhostKnightStruck;
                    this.DieSound = SoundIndex.GhostKnightDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.EmperorSaWoo.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.IcySpiritSpearman:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.AttackSound = SoundIndex.IcySpiritSpearmanAttack;
                    this.StruckSound = SoundIndex.IcySpiritSpearmanStruck;
                    this.DieSound = SoundIndex.IcySpiritSpearmanDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BanyaGuard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Werewolf:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.WerewolfAttack;
                    this.StruckSound = SoundIndex.WerewolfStruck;
                    this.DieSound = SoundIndex.WerewolfDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BanyaGuard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Whitefang:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.WhitefangAttack;
                    this.StruckSound = SoundIndex.WhitefangStruck;
                    this.DieSound = SoundIndex.WhitefangDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BanyaGuard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.IcySpiritSolider:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out this.BodyLibrary);
                    this.BodyShape = 9;
                    this.AttackSound = SoundIndex.IcySpiritSoliderAttack;
                    this.StruckSound = SoundIndex.IcySpiritSoliderStruck;
                    this.DieSound = SoundIndex.IcySpiritSoliderDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BanyaGuard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.WildBoar:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_18, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.WildBoarAttack;
                    this.StruckSound = SoundIndex.WildBoarStruck;
                    this.DieSound = SoundIndex.WildBoarDie;
                    break;
                case MonsterImage.JinamStoneGate:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out this.BodyLibrary);
                    this.BodyShape = 9;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.JinamStoneGate.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.FrostLordHwa:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.FrostLordHwaAttack;
                    this.StruckSound = SoundIndex.FrostLordHwaStruck;
                    this.DieSound = SoundIndex.FrostLordHwaDie;
                    break;
                case MonsterImage.Companion_Pig:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.Companion_TuskLord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out this.BodyLibrary);
                    this.BodyShape = 1;
                    break;
                case MonsterImage.Companion_SkeletonLord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out this.BodyLibrary);
                    this.BodyShape = 2;
                    break;
                case MonsterImage.Companion_Griffin:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out this.BodyLibrary);
                    this.BodyShape = 3;
                    break;
                case MonsterImage.Companion_Dragon:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out this.BodyLibrary);
                    this.BodyShape = 4;
                    break;
                case MonsterImage.Companion_Donkey:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out this.BodyLibrary);
                    this.BodyShape = 5;
                    break;
                case MonsterImage.Companion_Sheep:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out this.BodyLibrary);
                    this.BodyShape = 6;
                    break;
                case MonsterImage.Companion_BanyoLordGuzak:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out this.BodyLibrary);
                    this.BodyShape = 7;
                    break;
                case MonsterImage.Companion_Panda:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out this.BodyLibrary);
                    this.BodyShape = 8;
                    break;
                case MonsterImage.Companion_Rabbit:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_34, out this.BodyLibrary);
                    this.BodyShape = 9;
                    break;
                case MonsterImage.JinchonDevil:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_17, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.StruckSound = SoundIndex.JinchonDevilStruck;
                    this.DieSound = SoundIndex.JinchonDevilDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.JinchonDevil.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.OmaWarlord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_3, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.OmaHeroAttack;
                    this.StruckSound = SoundIndex.OmaHeroStruck;
                    this.DieSound = SoundIndex.OmaHeroDie;
                    break;
                case MonsterImage.EscortCommander:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_22, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.EscortCommanderAttack;
                    this.StruckSound = SoundIndex.EscortCommanderStruck;
                    this.DieSound = SoundIndex.EscortCommanderDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BanyaGuard.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.FieryDancer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_22, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.FieryDancerAttack;
                    this.StruckSound = SoundIndex.FieryDancerStruck;
                    this.DieSound = SoundIndex.FieryDancerDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.FieryDancer.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.EmeraldDancer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_22, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.EmeraldDancerAttack;
                    this.StruckSound = SoundIndex.EmeraldDancerStruck;
                    this.DieSound = SoundIndex.EmeraldDancerDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.EmeraldDancer.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.QueenOfDawn:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_22, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.QueenOfDawnAttack;
                    this.StruckSound = SoundIndex.QueenOfDawnStruck;
                    this.DieSound = SoundIndex.QueenOfDawnDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.QueenOfDawn.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.OYoungBeast:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.OYoungBeastAttack;
                    this.StruckSound = SoundIndex.OYoungBeastStruck;
                    this.DieSound = SoundIndex.OYoungBeastDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.OYoungBeast.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.YumgonWitch:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.AttackSound = SoundIndex.YumgonWitchAttack;
                    this.StruckSound = SoundIndex.YumgonWitchStruck;
                    this.DieSound = SoundIndex.YumgonWitchDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.YumgonWitch.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.MaWarlord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.MaWarlordAttack;
                    this.StruckSound = SoundIndex.MaWarlordStruck;
                    this.DieSound = SoundIndex.MaWarlordDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.OYoungBeast.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.JinhwanSpirit:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.JinhwanSpiritAttack;
                    this.StruckSound = SoundIndex.JinhwanSpiritStruck;
                    this.DieSound = SoundIndex.JinhwanSpiritDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.JinhwanSpirit.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.JinhwanGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out this.BodyLibrary);
                    this.BodyShape = 8;
                    this.AttackSound = SoundIndex.JinhwanGuardianAttack;
                    this.StruckSound = SoundIndex.JinhwanGuardianStruck;
                    this.DieSound = SoundIndex.JinhwanGuardianDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.JinhwanSpirit.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.YumgonGeneral:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.YumgonGeneralAttack;
                    this.StruckSound = SoundIndex.YumgonGeneralStruck;
                    this.DieSound = SoundIndex.YumgonGeneralDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.OYoungBeast.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.ChiwooGeneral:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.ChiwooGeneralAttack;
                    this.StruckSound = SoundIndex.ChiwooGeneralStruck;
                    this.DieSound = SoundIndex.ChiwooGeneralDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.ChiwooGeneral.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.DragonQueen:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.DragonQueenAttack;
                    this.StruckSound = SoundIndex.DragonQueenStruck;
                    this.DieSound = SoundIndex.DragonQueenDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.DragonQueen.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.DragonLord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_23, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.DragonLordAttack;
                    this.StruckSound = SoundIndex.DragonLordStruck;
                    this.DieSound = SoundIndex.DragonLordDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.DragonLord.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.FerociousIceTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_21, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.FerociousIceTigerAttack;
                    this.StruckSound = SoundIndex.FerociousIceTigerStruck;
                    this.DieSound = SoundIndex.FerociousIceTigerDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.FerociousIceTiger.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SamaFireGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.SamaFireGuardianAttack;
                    this.StruckSound = SoundIndex.SamaFireGuardianStruck;
                    this.DieSound = SoundIndex.SamaFireGuardianDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SamaFireGuardian.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SamaIceGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.SamaIceGuardianAttack;
                    this.StruckSound = SoundIndex.SamaIceGuardianStruck;
                    this.DieSound = SoundIndex.SamaIceGuardianDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SamaFireGuardian.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SamaLightningGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.SamaLightningGuardianAttack;
                    this.StruckSound = SoundIndex.SamaLightningGuardianStruck;
                    this.DieSound = SoundIndex.SamaLightningGuardianDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SamaFireGuardian.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SamaWindGuardian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.SamaWindGuardianAttack;
                    this.StruckSound = SoundIndex.SamaWindGuardianStruck;
                    this.DieSound = SoundIndex.SamaWindGuardianDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SamaFireGuardian.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Phoenix:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out this.BodyLibrary);
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.PhoenixAttack;
                    this.StruckSound = SoundIndex.PhoenixStruck;
                    this.DieSound = SoundIndex.PhoenixDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.Phoenix.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.BlackTortoise:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out this.BodyLibrary);
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.BlackTortoiseAttack;
                    this.StruckSound = SoundIndex.BlackTortoiseStruck;
                    this.DieSound = SoundIndex.BlackTortoiseDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.Phoenix.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.BlueDragon:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out this.BodyLibrary);
                    this.BodyShape = 6;
                    this.AttackSound = SoundIndex.BlueDragonAttack;
                    this.StruckSound = SoundIndex.BlueDragonStruck;
                    this.DieSound = SoundIndex.BlueDragonDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.Phoenix.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.WhiteTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_25, out this.BodyLibrary);
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.WhiteTigerAttack;
                    this.StruckSound = SoundIndex.WhiteTigerStruck;
                    this.DieSound = SoundIndex.WhiteTigerDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.Phoenix.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SamaCursedBladesman:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_27, out this.BodyLibrary);
                    this.BodyShape = 0;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SamaCursedBladesman.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SamaCursedSlave:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_27, out this.BodyLibrary);
                    this.BodyShape = 1;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SamaCursedSlave.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SamaCursedFlameMage:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_27, out this.BodyLibrary);
                    this.BodyShape = 2;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SamaCursedSlave.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SamaProphet:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_27, out this.BodyLibrary);
                    this.BodyShape = 3;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SamaProphet.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SamaSorcerer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_27, out this.BodyLibrary);
                    this.BodyShape = 4;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SamaSorcerer.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.EnshrinementBox:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_27, out this.BodyLibrary);
                    this.BodyShape = 5;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.EnshrinementBox.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.BloodStone:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_19, out this.BodyLibrary);
                    this.BodyShape = 7;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BloodStone.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.OrangeTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out this.BodyLibrary);
                    this.BodyShape = 0;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.OrangeTiger.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.RegularTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out this.BodyLibrary);
                    this.BodyShape = 1;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.OrangeTiger.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.RedTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out this.BodyLibrary);
                    this.BodyShape = 2;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.RedTiger.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SnowTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out this.BodyLibrary);
                    this.BodyShape = 3;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.OrangeTiger.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.BlackTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out this.BodyLibrary);
                    this.BodyShape = 4;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.OrangeTiger.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.BigBlackTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out this.BodyLibrary);
                    this.BodyShape = 5;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.OrangeTiger.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.BigWhiteTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out this.BodyLibrary);
                    this.BodyShape = 6;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.OrangeTiger.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.OrangeBossTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out this.BodyLibrary);
                    this.BodyShape = 7;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.OrangeBossTiger.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.BigBossTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_35, out this.BodyLibrary);
                    this.BodyShape = 8;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.OrangeBossTiger.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.WildMonkey:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_30, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.MonkeyAttack;
                    this.StruckSound = SoundIndex.MonkeyStruck;
                    this.DieSound = SoundIndex.MonkeyDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.Monkey.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.FrostYeti:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_30, out this.BodyLibrary);
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.ForestYetiAttack;
                    this.StruckSound = SoundIndex.ForestYetiStruck;
                    this.DieSound = SoundIndex.ForestYetiDie;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.ForestYeti.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.EvilSnake:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_9, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.ClawSerpentAttack;
                    this.StruckSound = SoundIndex.ClawSerpentStruck;
                    this.DieSound = SoundIndex.ClawSerpentDie;
                    break;
                case MonsterImage.Salamander:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_28, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.SandGolem:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_28, out this.BodyLibrary);
                    this.BodyShape = 1;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob3.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SDMob4:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_29, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.SDMob5:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_29, out this.BodyLibrary);
                    this.BodyShape = 1;
                    break;
                case MonsterImage.SDMob6:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_29, out this.BodyLibrary);
                    this.BodyShape = 2;
                    break;
                case MonsterImage.SDMob7:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_29, out this.BodyLibrary);
                    this.BodyShape = 8;
                    break;
                case MonsterImage.OmaMage:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_29, out this.BodyLibrary);
                    this.BodyShape = 9;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob8.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SDMob9:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out this.BodyLibrary);
                    this.BodyShape = 1;
                    break;
                case MonsterImage.SDMob10:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out this.BodyLibrary);
                    this.BodyShape = 5;
                    break;
                case MonsterImage.SDMob11:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out this.BodyLibrary);
                    this.BodyShape = 6;
                    break;
                case MonsterImage.SDMob12:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out this.BodyLibrary);
                    this.BodyShape = 7;
                    break;
                case MonsterImage.SDMob13:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out this.BodyLibrary);
                    this.BodyShape = 8;
                    break;
                case MonsterImage.SDMob14:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out this.BodyLibrary);
                    this.BodyShape = 9;
                    break;
                case MonsterImage.CrystalGolem:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_40, out this.BodyLibrary);
                    this.BodyShape = 0;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob15.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.DustDevil:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_41, out this.BodyLibrary);
                    this.BodyShape = 1;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob16.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.TwinTailScorpion:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_41, out this.BodyLibrary);
                    this.BodyShape = 2;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob17.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.BloodyMole:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_41, out this.BodyLibrary);
                    this.BodyShape = 3;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob18.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SDMob19:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out this.BodyLibrary);
                    this.BodyShape = 3;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob19.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SDMob20:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out this.BodyLibrary);
                    this.BodyShape = 4;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob19.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SDMob21:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out this.BodyLibrary);
                    this.BodyShape = 5;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob21.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SDMob22:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out this.BodyLibrary);
                    this.BodyShape = 6;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob22.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SDMob23:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out this.BodyLibrary);
                    this.BodyShape = 7;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob23.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SDMob24:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out this.BodyLibrary);
                    this.BodyShape = 8;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob24.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.SDMob25:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_44, out this.BodyLibrary);
                    this.BodyShape = 9;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob25.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.GangSpider:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_28, out this.BodyLibrary);
                    this.BodyShape = 8;
                    break;
                case MonsterImage.VenomSpider:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_28, out this.BodyLibrary);
                    this.BodyShape = 9;
                    break;
                case MonsterImage.SDMob26:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_45, out this.BodyLibrary);
                    this.BodyShape = 0;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob26.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.LobsterLord:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_45, out this.BodyLibrary);
                    this.BodyShape = 3;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.LobsterLord.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.NewMob1:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.NewMob2:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out this.BodyLibrary);
                    this.BodyShape = 1;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.BobbitWorm.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.NewMob3:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out this.BodyLibrary);
                    this.BodyShape = 2;
                    break;
                case MonsterImage.NewMob4:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out this.BodyLibrary);
                    this.BodyShape = 3;
                    break;
                case MonsterImage.NewMob5:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out this.BodyLibrary);
                    this.BodyShape = 4;
                    break;
                case MonsterImage.NewMob6:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out this.BodyLibrary);
                    this.BodyShape = 5;
                    break;
                case MonsterImage.NewMob7:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out this.BodyLibrary);
                    this.BodyShape = 6;
                    break;
                case MonsterImage.NewMob8:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out this.BodyLibrary);
                    this.BodyShape = 7;
                    break;
                case MonsterImage.NewMob9:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out this.BodyLibrary);
                    this.BodyShape = 8;
                    break;
                case MonsterImage.NewMob10:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_47, out this.BodyLibrary);
                    this.BodyShape = 9;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.DeadTree.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.MonasteryMon0:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.MonasteryMon1:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out this.BodyLibrary);
                    this.BodyShape = 1;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.MonasteryMon1.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.MonasteryMon2:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out this.BodyLibrary);
                    this.BodyShape = 2;
                    break;
                case MonsterImage.MonasteryMon3:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out this.BodyLibrary);
                    this.BodyShape = 3;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.MonasteryMon3.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.MonasteryMon4:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out this.BodyLibrary);
                    this.BodyShape = 4;
                    break;
                case MonsterImage.MonasteryMon5:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out this.BodyLibrary);
                    this.BodyShape = 5;
                    break;
                case MonsterImage.MonasteryMon6:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_49, out this.BodyLibrary);
                    this.BodyShape = 6;
                    break;
                case MonsterImage.Taishan01:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_24, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.Taishan02:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_24, out this.BodyLibrary);
                    this.BodyShape = 1;
                    break;
                case MonsterImage.Taishan03:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_24, out this.BodyLibrary);
                    this.BodyShape = 2;
                    break;
                case MonsterImage.Taishan04:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_24, out this.BodyLibrary);
                    this.BodyShape = 3;
                    break;
                case MonsterImage.Taishan05:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_24, out this.BodyLibrary);
                    this.BodyShape = 4;
                    break;
                case MonsterImage.Taishan06:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_24, out this.BodyLibrary);
                    this.BodyShape = 5;
                    break;
                case MonsterImage.Taishan07:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_24, out this.BodyLibrary);
                    this.BodyShape = 6;
                    break;
                case MonsterImage.Yuehe00:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_31, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.Yuehe01:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_31, out this.BodyLibrary);
                    this.BodyShape = 1;
                    break;
                case MonsterImage.Yuehe02:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_31, out this.BodyLibrary);
                    this.BodyShape = 2;
                    break;
                case MonsterImage.Yuehe03:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_31, out this.BodyLibrary);
                    this.BodyShape = 3;
                    break;
                case MonsterImage.Yuehe04:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_31, out this.BodyLibrary);
                    this.BodyShape = 4;
                    break;
                case MonsterImage.Yuehe05:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_31, out this.BodyLibrary);
                    this.BodyShape = 5;
                    break;
                case MonsterImage.Yuehe06:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_31, out this.BodyLibrary);
                    this.BodyShape = 6;
                    break;
                case MonsterImage.Dashewan:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_33, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.GardenSoldier:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_38, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.GardenSoldier)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 0;
                    this.StruckSound = SoundIndex.GardenSoldierStruck;
                    this.DieSound = SoundIndex.GardenSoldierDie;
                    break;
                case MonsterImage.GardenDefender:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_38, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.GardenDefender)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 1;
                    this.StruckSound = SoundIndex.GardenDefenderStruck;
                    this.DieSound = SoundIndex.GardenDefenderDie;
                    break;
                case MonsterImage.RedBlossom:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_38, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.RedBlossom)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 2;
                    this.StruckSound = SoundIndex.RedBlossomStruck;
                    this.DieSound = SoundIndex.RedBlossomDie;
                    break;
                case MonsterImage.BlueBlossom:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_38, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.BlueBlossom)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.BlueBlossomAttack;
                    this.StruckSound = SoundIndex.BlueBlossomStruck;
                    this.DieSound = SoundIndex.BlueBlossomDie;
                    break;
                case MonsterImage.FireBird:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_38, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.FireBird)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 4;
                    this.StruckSound = SoundIndex.FireBirdStruck;
                    this.DieSound = SoundIndex.FireBirdDie;
                    break;
                case MonsterImage.Taohua06:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_38, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.GardenDefender)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 5;
                    break;
                case MonsterImage.Taohua07:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_38, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.GardenDefender)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 6;
                    break;
                case MonsterImage.Taohua08:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_38, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.Hehua)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 7;
                    break;
                case MonsterImage.Taohua09:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_38, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.FireBird)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 8;
                    break;
                case MonsterImage.Benma01:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_39, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.Benma02:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_40, out this.BodyLibrary);
                    this.BodyShape = 6;
                    break;
                case MonsterImage.Benma03:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_40, out this.BodyLibrary);
                    this.BodyShape = 7;
                    break;
                case MonsterImage.Qinling01:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_42, out this.BodyLibrary);
                    this.BodyShape = 0;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob28.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Qinling02:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_42, out this.BodyLibrary);
                    this.BodyShape = 1;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob27.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Qinling03:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_42, out this.BodyLibrary);
                    this.BodyShape = 2;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.Xianjin.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Qinling04:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_42, out this.BodyLibrary);
                    this.BodyShape = 3;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.Changmingdeng.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Qinling05:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_42, out this.BodyLibrary);
                    this.BodyShape = 4;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob29.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Qinling06:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_42, out this.BodyLibrary);
                    this.BodyShape = 5;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob29.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Qinling07:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_42, out this.BodyLibrary);
                    this.BodyShape = 6;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob29.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Qinling08:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_42, out this.BodyLibrary);
                    this.BodyShape = 7;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob29.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Qinling09:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_42, out this.BodyLibrary);
                    this.BodyShape = 8;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.SDMob29.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Qinling10:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_42, out this.BodyLibrary);
                    this.BodyShape = 9;
                    break;
                case MonsterImage.Companion_Snow:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_41, out this.BodyLibrary);
                    this.BodyShape = 7;
                    break;
                case MonsterImage.CrazedPrimate:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_36, out this.BodyLibrary);
                    this.BodyShape = 2;
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.CrazedPrimate)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.AttackSound = SoundIndex.CrazedPrimateAttack;
                    this.StruckSound = SoundIndex.CrazedPrimateStruck;
                    this.DieSound = SoundIndex.CrazedPrimateDie;
                    break;
                case MonsterImage.HellBringer:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_36, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.HellBringer)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 3;
                    this.StruckSound = SoundIndex.HellBringerStruck;
                    this.DieSound = SoundIndex.HellBringerDie;
                    break;
                case MonsterImage.YurinMon0:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_37, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.YurinMon0)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.YurinHoundAttack;
                    this.StruckSound = SoundIndex.YurinHoundStruck;
                    this.DieSound = SoundIndex.YurinHoundDie;
                    break;
                case MonsterImage.YurinMon1:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_37, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.YurinMon1)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 1;
                    this.AttackSound = SoundIndex.YurinHoundAttack;
                    this.StruckSound = SoundIndex.YurinHoundStruck;
                    this.DieSound = SoundIndex.YurinHoundDie;
                    break;
                case MonsterImage.WhiteBeardedTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_37, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.WhiteBeardedTiger)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.YurinTigerAttack;
                    this.StruckSound = SoundIndex.YurinTigerStruck;
                    this.DieSound = SoundIndex.YurinTigerDie;
                    break;
                case MonsterImage.BlackBeardedTiger:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_37, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.WhiteBeardedTiger)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.YurinTigerAttack;
                    this.StruckSound = SoundIndex.YurinTigerStruck;
                    this.DieSound = SoundIndex.YurinTigerDie;
                    break;
                case MonsterImage.HardenedRhino:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_37, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.HardenedRhino)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 4;
                    this.AttackSound = SoundIndex.HardenedRhinoAttack;
                    this.StruckSound = SoundIndex.HardenedRhinoStruck;
                    this.DieSound = SoundIndex.HardenedRhinoDie;
                    break;
                case MonsterImage.Mammoth:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_37, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.Mammoth)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 5;
                    this.AttackSound = SoundIndex.MammothAttack;
                    this.StruckSound = SoundIndex.MammothStruck;
                    this.DieSound = SoundIndex.MammothDie;
                    break;
                case MonsterImage.CursedSlave1:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_37, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.CursedSlave1)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 6;
                    this.StruckSound = SoundIndex.CursedSlave1Struck;
                    this.DieSound = SoundIndex.CursedSlave1Die;
                    break;
                case MonsterImage.CursedSlave2:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_37, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.CursedSlave2)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 7;
                    this.AttackSound = SoundIndex.CursedSlave2Attack;
                    this.StruckSound = SoundIndex.CursedSlave2Struck;
                    this.DieSound = SoundIndex.CursedSlave2Die;
                    break;
                case MonsterImage.CursedSlave3:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_37, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.CursedSlave3)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 8;
                    this.StruckSound = SoundIndex.CursedSlave3Struck;
                    this.DieSound = SoundIndex.CursedSlave3Die;
                    break;
                case MonsterImage.PoisonousGolem:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_37, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.PoisonousGolem)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 9;
                    this.StruckSound = SoundIndex.PoisonousGolemStruck;
                    this.DieSound = SoundIndex.PoisonousGolemDie;
                    break;
                case MonsterImage.Huanjingsamll:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_33, out this.BodyLibrary);
                    this.BodyShape = 2;
                    break;
                case MonsterImage.Luwang:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out this.BodyLibrary);
                    this.BodyShape = 1;
                    break;
                case MonsterImage.Yangling:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_50, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.Zhandouji:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_56, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.Wolong1:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.Wolong2:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out this.BodyLibrary);
                    this.BodyShape = 1;
                    break;
                case MonsterImage.Wolong3:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out this.BodyLibrary);
                    this.BodyShape = 2;
                    break;
                case MonsterImage.Wolong4:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out this.BodyLibrary);
                    this.BodyShape = 3;
                    break;
                case MonsterImage.Wolong5:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_32, out this.BodyLibrary);
                    this.BodyShape = 4;
                    break;
                case MonsterImage.ShengdanMan:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out this.BodyLibrary);
                    this.BodyShape = 5;
                    break;
                case MonsterImage.ShengdanTree:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out this.BodyLibrary);
                    this.BodyShape = 3;
                    break;
                case MonsterImage.Haidi01:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_53, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.Haidi02:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_53, out this.BodyLibrary);
                    this.BodyShape = 1;
                    break;
                case MonsterImage.Haidi03:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_53, out this.BodyLibrary);
                    this.BodyShape = 2;
                    break;
                case MonsterImage.Haidi04:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_53, out this.BodyLibrary);
                    this.BodyShape = 3;
                    break;
                case MonsterImage.Haidi05:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_53, out this.BodyLibrary);
                    this.BodyShape = 4;
                    break;
                case MonsterImage.Haidi06:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_53, out this.BodyLibrary);
                    this.BodyShape = 5;
                    break;
                case MonsterImage.Haidi07:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_53, out this.BodyLibrary);
                    this.BodyShape = 6;
                    break;
                case MonsterImage.Zhangyu:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_53, out this.BodyLibrary);
                    this.BodyShape = 7;
                    break;
                case MonsterImage.Yanhua:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_59, out this.BodyLibrary);
                    this.BodyShape = 0;
                    using (Dictionary<MirAnimation, Frame>.Enumerator enumerator = FrameSet.Yanhua.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            KeyValuePair<MirAnimation, Frame> current = enumerator.Current;
                            this.Frames[current.Key] = current.Value;
                        }
                        break;
                    }
                case MonsterImage.Nian:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_46, out this.BodyLibrary);
                    this.BodyShape = 1;
                    break;
                case MonsterImage.Bindu:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_60, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.Qitiandashen:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_61, out this.BodyLibrary);
                    foreach (KeyValuePair<MirAnimation, Frame> keyValuePair in FrameSet.QitiandashenFrame)
                        this.Frames[keyValuePair.Key] = keyValuePair.Value;
                    this.BodyShape = 0;
                    this.StruckSound = SoundIndex.CursedSlave3Struck;
                    this.DieSound = SoundIndex.CursedSlave3Die;
                    break;
                case MonsterImage.NewSenlingXueren:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_16, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.ForestYetiAttack;
                    this.StruckSound = SoundIndex.ForestYetiStruck;
                    this.DieSound = SoundIndex.ForestYetiDie;
                    break;
                case MonsterImage.红狐:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out this.BodyLibrary);
                    this.BodyShape = 0;
                    this.AttackSound = SoundIndex.SpiderBatAttack;
                    this.StruckSound = SoundIndex.SpiderBatStruck;
                    this.DieSound = SoundIndex.SpiderBatDie;
                    break;
                case MonsterImage.白狐:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out this.BodyLibrary);
                    this.BodyShape = 2;
                    this.AttackSound = SoundIndex.SpiderBatAttack;
                    this.StruckSound = SoundIndex.SpiderBatStruck;
                    this.DieSound = SoundIndex.SpiderBatDie;
                    break;
                case MonsterImage.黄狐:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_11, out this.BodyLibrary);
                    this.BodyShape = 3;
                    this.AttackSound = SoundIndex.SpiderBatAttack;
                    this.StruckSound = SoundIndex.SpiderBatStruck;
                    this.DieSound = SoundIndex.SpiderBatDie;
                    break;
                case MonsterImage.镜中仙:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_43, out this.BodyLibrary);
                    this.BodyShape = 3;
                    break;
                case MonsterImage.五雷使:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_43, out this.BodyLibrary);
                    this.BodyShape = 4;
                    break;
                case MonsterImage.白泽:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_51, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
                case MonsterImage.混沌:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_57, out this.BodyLibrary);
                    this.BodyShape = 8;
                    break;
                case MonsterImage.Mon63_1:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_63, out this.BodyLibrary);
                    this.BodyShape = 1;
                    break;
                case MonsterImage.Mon63_2:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_63, out this.BodyLibrary);
                    this.BodyShape = 2;
                    break;
                case MonsterImage.Mon63_5:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_63, out this.BodyLibrary);
                    this.BodyShape = 5;
                    break;
                case MonsterImage.Mon63_6:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_63, out this.BodyLibrary);
                    this.BodyShape = 6;
                    break;
                case MonsterImage.Mon63_7:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_63, out this.BodyLibrary);
                    this.BodyShape = 7;
                    break;
                case MonsterImage.Mon63_8:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_63, out this.BodyLibrary);
                    this.BodyShape = 8;
                    break;
                case MonsterImage.Mon63_9:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_63, out this.BodyLibrary);
                    this.BodyShape = 9;
                    break;
                default:
                    CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out this.BodyLibrary);
                    this.BodyShape = 0;
                    break;
            }

            #endregion

            if (EasterEvent)
            {
                CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_30, out BodyLibrary);
                BodyShape = 4;


                Frames = new Dictionary<MirAnimation, Frame>(FrameSet.DefaultMonster);

                foreach (KeyValuePair<MirAnimation, Frame> frame in FrameSet.EasterEvent)
                    Frames[frame.Key] = frame.Value;
            }
            else if (HalloweenEvent)
            {
                CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_1, out BodyLibrary);
                BodyShape = 1;


                Frames = new Dictionary<MirAnimation, Frame>(FrameSet.DefaultMonster);
            }
            else if (ChristmasEvent)
            {
                CEnvir.LibraryList.TryGetValue(LibraryFile.Mon_20, out BodyLibrary);
                BodyShape = 0;


                Frames = new Dictionary<MirAnimation, Frame>(FrameSet.DefaultMonster);
            }
        }


        public override void SetAnimation(ObjectAction action)
        {
            MirAnimation animation;
            MagicType type;

            switch (action.Action)
            {
                case MirAction.Standing:
                    switch (Image)
                    {
                            case MonsterImage.ZumaGuardian:
                            case MonsterImage.ZumaFanatic:
                            case MonsterImage.ZumaKing:
                            animation = !Extra ? MirAnimation.StoneStanding : MirAnimation.Standing;
                            break;
                        default:
                                animation = MirAnimation.Standing;

                            if (VisibleBuffs.Contains(BuffType.DragonRepulse))
                                animation = MirAnimation.DragonRepulseMiddle;
                            else if (CurrentAnimation == MirAnimation.DragonRepulseMiddle)
                                animation = MirAnimation.DragonRepulseEnd;
                            break;
                    }
                    break;
                case MirAction.Moving:
                    animation = MirAnimation.Walking;
                    break;
                case MirAction.Pushed:
                    animation = MirAnimation.Pushed;
                    break;
                case MirAction.Attack:
                    animation = MirAnimation.Combat1;
                    break;
                case MirAction.RangeAttack:
                    animation = MirAnimation.Combat2;
                    break;
                case MirAction.Spell:
                    type = (MagicType)action.Extra[0];
                    
                    animation = MirAnimation.Combat3;

                    if (type == MagicType.DragonRepulse)
                        animation = MirAnimation.DragonRepulseStart;

                    switch (type)
                    {
                        case MagicType.DoomClawRightPinch:
                            animation = MirAnimation.Combat1;
                            break;
                        case MagicType.DoomClawRightSwipe:
                            animation = MirAnimation.Combat2;
                            break;
                        case MagicType.DoomClawSpit:
                            animation = MirAnimation.Combat7;
                            break;
                        case MagicType.DoomClawWave:
                            animation = MirAnimation.Combat6;
                            break;
                        case MagicType.DoomClawLeftPinch:
                            animation = MirAnimation.Combat4;
                            break;
                        case MagicType.DoomClawLeftSwipe:
                            animation = MirAnimation.Combat5;
                            break;
                    }
                    break;
             //   case MirAction.Struck:
             //       animation = MirAnimation.Struck;
             //       break;
                case MirAction.Die:
                    animation = MirAnimation.Die;
                    break;
                case MirAction.Dead:
                    animation = !Skeleton ? MirAnimation.Dead : MirAnimation.Skeleton;
                    break;
                case MirAction.Show:
                    animation = MirAnimation.Show;
                    break;
                case MirAction.Hide:
                    animation = MirAnimation.Hide;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            CurrentAnimation = animation;
            if (!Frames.TryGetValue(CurrentAnimation, out CurrentFrame))
                CurrentFrame = Frame.EmptyFrame;
        }

        public override void Draw()
        {
            if (BodyLibrary == null || !Visible) return;

            int y = DrawY;

            switch (Image)
            {
                case MonsterImage.ChestnutTree:
                    y -= MapControl.CellHeight;
                    break;
                case MonsterImage.NewMob10:
                    y -= MapControl.CellHeight * 4;
                    break;
            }

            DrawShadow(DrawX, y);


            DrawBody(DrawX, y);
        }

        public void DrawShadow(int x, int y)
        {
            switch (Image)
            {
                case MonsterImage.DustDevil:
                    break;
                case MonsterImage.LobsterLord:
                    BodyLibrary.Draw(BodyFrame, x, y, Color.White, true, 0.5f, ImageType.Shadow);
                    BodyLibrary.Draw(BodyFrame + 1000, x, y, Color.White, true, 0.5f, ImageType.Shadow);
                    BodyLibrary.Draw(BodyFrame + 2000, x, y, Color.White, true, 0.5f, ImageType.Shadow);
                    break;
                default:
                    BodyLibrary.Draw(BodyFrame, x, y, Color.White, true, 0.5f, ImageType.Shadow);
                    break;
            }

        }
        public void DrawBody(int x, int y)
        {
            switch (Image)
            {
                case MonsterImage.DustDevil:
                    BodyLibrary.DrawBlend(BodyFrame, x, y, DrawColour, true, Opacity, ImageType.Image);
                    break;
                case MonsterImage.LobsterLord:
                    BodyLibrary.Draw(BodyFrame, x, y, DrawColour, true, Opacity, ImageType.Image);
                    BodyLibrary.Draw(BodyFrame + 1000, x, y, DrawColour, true, Opacity, ImageType.Image);
                    BodyLibrary.Draw(BodyFrame + 2000, x, y, DrawColour, true, Opacity, ImageType.Image);
                    break;
                default:
                    BodyLibrary.Draw(BodyFrame, x, y, DrawColour, true, Opacity, ImageType.Image);
                    break;
            }


            MirLibrary library;
            switch (Image)
            {
                case MonsterImage.NewMob1:
                    if (CurrentAction == MirAction.Dead) break;
                    if (!CEnvir.LibraryList.TryGetValue(LibraryFile.MonMagicEx20, out library)) break;
                    library.DrawBlend(DrawFrame + 2000, x, y, Color.White, true, 1f, ImageType.Image);
                    break;
                case MonsterImage.NumaHighMage:
                    if (CurrentAction == MirAction.Dead) break;
                    if (!CEnvir.LibraryList.TryGetValue(LibraryFile.MonMagicEx4, out library)) break;
                    library.DrawBlend(DrawFrame + 500, x, y, Color.White, true, 1f, ImageType.Image);
                    break;
                case MonsterImage.InfernalSoldier:
                    if (CurrentAction == MirAction.Dead) break;
                    if (!CEnvir.LibraryList.TryGetValue(LibraryFile.MonMagicEx8, out library)) break;
                    library.DrawBlend(DrawFrame, x, y, Color.White, true, 1f, ImageType.Image);
                    break;
                case MonsterImage.JinamStoneGate:
                    if (CurrentAction == MirAction.Dead) break;
                    if (!CEnvir.LibraryList.TryGetValue(LibraryFile.MonMagicEx6, out library)) break;
                    library.DrawBlend((GameScene.Game.MapControl.Animation % 30) + 1400, x, y, Color.White, true, 1f, ImageType.Image);
                    break;

            }
        }
        public override void DrawHealth()
        {
            if (!Config.ShowMonsterHealth || ((CEnvir.Now > DrawHealthTime || !Visible) && PetOwner != User.Name)) return;

            if (MonsterInfo.AI < 0) return;

            ClientObjectData data;
            if (!GameScene.Game.DataDictionary.TryGetValue(ObjectID, out data)) return;

            if (data.MaxHealth == 0) return;

            MirLibrary library;

            if (!CEnvir.LibraryList.TryGetValue(LibraryFile.Interface, out library)) return;
            
            
            float percent = Math.Min(1, Math.Max(0, data.Health / (float)data.MaxHealth));

            if (percent == 0) return;

            Size size = library.GetSize(79);

            Color color = !string.IsNullOrEmpty(PetOwner) ? Color.Yellow : Color.FromArgb(0, 200, 74);

            library.Draw(80, DrawX, DrawY - 55, Color.White, false, 1F, ImageType.Image);
            library.Draw(79, DrawX + 1, DrawY - 55 + 1, color, new Rectangle(0, 0, (int) (size.Width*percent), size.Height), 1F, ImageType.Image);
          }
        public override void DrawBlend()
        {
            if (BodyLibrary == null || !Visible) return;

            int y = DrawY;

            switch (Image)
            {
                case MonsterImage.ChestnutTree:
                    y -= MapControl.CellHeight;
                    break;
                    case MonsterImage.JinamStoneGate:
                    return;
            }
            DXManager.SetBlend(true, 0.60F);
            DrawBody(DrawX, y);
            DXManager.SetBlend(false);
        }
        public override void DrawName()
        {
            if (!Visible) return;

            base.DrawName();
        }

        public override void CreateProjectile()
        {
            base.CreateProjectile();

            switch (Image)
            {
                case MonsterImage.SkeletonAxeThrower:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(800, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                        });
                    }
                    break;

                case MonsterImage.AntNeedler:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(80, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Skip = 0,
                        });
                    }
                    break;

                case MonsterImage.AntHealer:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(100, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.HolyColour)
                        {
                            Target = attackTarget,
                            Skip = 0,
                            Blend = true,
                        });
                    }
                    break;

                case MonsterImage.SpinedDarkLizard:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(1240, 1, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Skip = 10,
                        });
                    }
                    break;

                case MonsterImage.RedMoonTheFallen:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(2230, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour)
                        {
                            MapTarget = attackTarget.CurrentLocation,
                            Skip = 0,
                        });
                    }
                    break;

                case MonsterImage.ZumaSharpShooter:
                case MonsterImage.BoneArcher:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(1070, 1, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Skip = 10,
                        });
                    }
                    break;
                case MonsterImage.Monkey:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(900, 1, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx2, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Skip = 10,
                            Has16Directions = false,
                        });
                    }
                    break;
                case MonsterImage.CannibalFanatic:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(0, 1, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx2, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                        });
                    }
                    break;
                case MonsterImage.CursedCactus:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(960, 1, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Skip = 0,
                        });
                    }
                    break;
                case MonsterImage.WindfurySorceress:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(1570, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 60, Globals.WindColour)
                        {
                            Target = attackTarget,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.SonicLizard:
                    Effects.Add(new MirEffect(1444, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx4, 20, 60, Globals.FireColour)
                    {
                        Target = this,
                        Blend = true,
                        Direction = Direction,

                    });
                    break;
                case MonsterImage.GiantLizard:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(5930, 4, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx4, 0, 0, Globals.NoneColour)
                        {
                            MapTarget = attackTarget.CurrentLocation,
                        });
                    }
                    break;
                case MonsterImage.CrazedLizard:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(5830, 3, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx4, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.EmperorSaWoo:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(600, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 60, 60, Globals.WindColour)
                        {
                            MapTarget = attackTarget.CurrentLocation,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.ArchLichTaedu:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(420, 5, TimeSpan.FromMilliseconds(100), LibraryFile.Magic, 30, 50, Globals.FireColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.RazorTusk:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(1890, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 30, 50, Globals.WindColour)
                        {
                            MapTarget = attackTarget.CurrentLocation,
                            Blend = true,
                            Direction = attackTarget.Direction,
                            BlendRate = 1F,
                        });
                    }
                    break;
                case MonsterImage.MutantCaptain:
                    Effects.Add(new MirEffect(560, 9, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx2, 20, 60, Globals.FireColour)
                    {
                        Target = this,
                        Blend = true,
                        Direction = Direction,
                    });
                    break;
                case MonsterImage.StoneGriffin:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(1080, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx2, 0, 0, Globals.DarkColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.FlameGriffin:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(1080, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx2, 0, 0, Globals.FireColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Blend = true,
                            DrawColour = Color.Orange,
                        });
                    }
                    break;
                case MonsterImage.NumaCavalry:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(0, 4, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx4, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                            Skip = 10,
                        });
                    }
                    break;
                case MonsterImage.NumaStoneThrower:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        MirEffect effect;
                        Effects.Add(effect = new MirProjectile(0, 4, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx3, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                            Skip = 10,
                        });

                        effect.CompleteAction = () =>
                        {
                            attackTarget.Effects.Add(effect = new MirEffect(80, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx3, 10, 35, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = attackTarget,
                            });
                            effect.Process();

                            DXSoundManager.Play(SoundIndex.FireStormEnd);
                        };
                        effect.Process();
                    }
                    break;
                case MonsterImage.NumaRoyalGuard:
                    Effects.Add(new MirEffect(1440, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx4, 20, 60, Globals.FireColour)
                    {
                        Target = this,
                        Blend = true,
                        Direction = Direction,
                    });
                    break;
                case MonsterImage.IcyGoddess:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(6200, 5, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx3, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                            Skip = 0,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.IcySpiritGeneral:
                case MonsterImage.IcySpiritWarrior:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirEffect(580, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx3, 0, 0, Globals.NoneColour)
                        {
                            Target = attackTarget,
                            Blend = true,
                        });
                    }
                    break;
                case MonsterImage.EvilElephant:
                case MonsterImage.SandShark:
                    Effects.Add(new MirEffect(320, 10, TimeSpan.FromMilliseconds(80), LibraryFile.MonMagic, 10, 35, Globals.DarkColour)
                    {
                        Blend = true,
                        Target = this,
                    });
                    break;
                case MonsterImage.GhostKnight:
                    Effects.Add(new MirEffect(6350, 10, TimeSpan.FromMilliseconds(80), LibraryFile.MonMagicEx3, 10, 35, Globals.DarkColour)
                    {
                        Blend = true,
                        Target = this,
                    });
                    break;
                case MonsterImage.IcyRanger:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        Effects.Add(new MirProjectile(190, 1, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx3, 0, 0, Globals.NoneColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                        });
                    }
                    break;
                case MonsterImage.YumgonWitch:
                    Effects.Add(new MirEffect(20, 18, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 20, 60, Globals.LightningColour)
                    {
                        Target = this,
                        Blend = true,
                    });
                    break;
                case MonsterImage.ChiwooGeneral:
                    Effects.Add(new MirEffect(1000, 15, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 0, 0, Globals.NoneColour)
                    {
                        Target = this,
                        Blend = true,
                    });
                    break;
                case MonsterImage.DragonLord:
                    foreach (MapObject target in AttackTargets)
                    {
                        MirProjectile eff;
                        Point p = new Point(target.CurrentLocation.X +4, target.CurrentLocation.Y - 10);
                        Effects.Add(eff = new MirProjectile(130, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 0, 0, Globals.NoneColour, p)
                        {
                            MapTarget = target.CurrentLocation,
                            Skip = 0,
                            Explode = true,
                            Blend =  true,
                        });

                        eff.CompleteAction = () =>
                        {
                            Effects.Add(new MirEffect(140, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 0, 0, Globals.NoneColour)
                            {
                                MapTarget = eff.MapTarget,
                                Blend = true,
                            });
                        };
                    }
                    break;
                case MonsterImage.SamaCursedFlameMage:
                    foreach (MapObject attackTarget in AttackTargets)
                    {
                        MirEffect effect;
                        Effects.Add(effect = new MirProjectile(5000, 9, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 0, 0, Globals.FireColour, CurrentLocation)
                        {
                            Target = attackTarget,
                            Has16Directions = false,
                            Skip = 10,
                            Blend = true,
                        });

                        effect.CompleteAction = () =>
                        {
                            attackTarget.Effects.Add(effect = new MirEffect(5100, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx9, 10, 35, Globals.FireColour)
                            {
                                Blend = true,
                                Target = attackTarget,
                            });
                            effect.Process();
                        };
                        effect.Process();
                    }
                    break;
            }

        }

        public override bool MouseOver(Point p)
        {
            if (!Visible || BodyLibrary == null) return false;

            switch (Image)
            {
                case MonsterImage.LobsterLord:
                        return BodyLibrary.VisiblePixel(BodyFrame, new Point(p.X - DrawX, p.Y - DrawY), false, true) ||
                               BodyLibrary.VisiblePixel(BodyFrame + 1000, new Point(p.X - DrawX, p.Y - DrawY), false, true) ||
                               BodyLibrary.VisiblePixel(BodyFrame + 2000, new Point(p.X - DrawX, p.Y - DrawY), false, true);
                default:
                    return BodyLibrary.VisiblePixel(BodyFrame, new Point(p.X - DrawX, p.Y - DrawY), false, true);
            }

        }

        public override void SetAction(ObjectAction action)
        {
            switch (Image)
            {
                case MonsterImage.Shinsu:
                    switch (CurrentAction) // Old Action
                    {
                        case MirAction.Hide:
                            Extra = false;
                            UpdateLibraries();
                            break;
                        case MirAction.Dead:
                            Visible = true;
                            break;
                    }
                    switch (action.Action) //Mew Action
                    {
                        case MirAction.Show:
                            Extra = true;
                            DXSoundManager.Play(SoundIndex.ShinsuShow);
                            UpdateLibraries();
                            break;
                        case MirAction.Hide:
                            DXSoundManager.Play(SoundIndex.ShinsuBigAttack);
                            break;
                        case MirAction.Dead:
                            Visible = false;
                            break;
                    }
                    break;
                case MonsterImage.InfernalSoldier:
                    switch (CurrentAction) // Old Action
                    {
                        case MirAction.Dead:
                            Visible = true;
                            break;
                    }
                    switch (action.Action) //Mew Action
                    {
                        case MirAction.Dead:
                            Visible = false;
                            break;
                    }
                    break;
                case MonsterImage.CarnivorousPlant:
                case MonsterImage.LordNiJae:
                    if (CurrentAction == MirAction.Hide)
                        Visible = false;

                    if (action.Action == MirAction.Show)
                        Visible = true;
                    break;
                case MonsterImage.GhostMage:
                    switch (action.Action)
                    {
                        case MirAction.Show:
                            DXSoundManager.Play(SoundIndex.GhostMageAppear);
                            new MirEffect(240, 1, TimeSpan.FromMinutes(1), LibraryFile.ProgUse, 0, 0, Globals.NoneColour)
                            {
                                MapTarget = action.Location,
                                DrawType = DrawType.Floor,
                                Direction = Direction,
                                Skip = 1,
                            };
                            break;
                    }
                    break;
                case MonsterImage.StoneGolem:
                    switch (action.Action)
                    {
                        case MirAction.Show:
                            DXSoundManager.Play(SoundIndex.StoneGolemAppear);
                            new MirEffect(200, 1, TimeSpan.FromMinutes(1), LibraryFile.ProgUse, 0, 0, Globals.NoneColour)
                            {
                                MapTarget = action.Location,
                                DrawType = DrawType.Floor,
                                Direction = Direction,
                                Skip = 1,
                            };
                            break;
                    }
                    break;
                case MonsterImage.ZumaFanatic:
                case MonsterImage.ZumaGuardian:
                    switch (CurrentAction) 
                    {
                        case MirAction.Show:
                            Extra = true;
                            break;
                    }
                    break;
                case MonsterImage.ZumaKing:
                    switch (CurrentAction)
                    {
                        case MirAction.Show:
                            Extra = true;
                            new MirEffect(210, 1, TimeSpan.FromMinutes(1), LibraryFile.ProgUse, 0, 0, Globals.NoneColour)
                            {
                                MapTarget = action.Location,
                                DrawType = DrawType.Floor,
                            };
                            break;
                    }
                    break;
            }

            base.SetAction(action);

            switch (Image)
            {
                case MonsterImage.Scarecrow:
                    switch (action.Action)
                    {
                        case MirAction.Die:
                            Effects.Add(new MirEffect(680, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.FireColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.Skeleton:
                case MonsterImage.SkeletonAxeThrower:
                case MonsterImage.SkeletonWarrior:
                case MonsterImage.SkeletonLord:
                    switch (action.Action)
                    {
                        case MirAction.Die:
                            Effects.Add(new MirEffect(1920, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.FireColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.GhostSorcerer:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(600, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.LightningColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Die:
                            Effects.Add(new MirEffect(700, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.LightningColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.CaveMaggot:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(1940, 5, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.DarkColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(200),
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.LordNiJae:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            MirEffect effect;
                            Effects.Add(effect = new MirEffect(361, 9, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.DarkColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(400),
                                Blend = true,
                            });
                            effect.Process();

                            break;
                    }
                    break;
                case MonsterImage.RottingGhoul:
                    switch (action.Action)
                    {
                        case MirAction.Die:
                            Effects.Add(new MirEffect(490, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 20, 40, Globals.LightningColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.DecayingGhoul:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(310, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 20, 40, Globals.LightningColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(400),
                                Blend = true,
                            });
                            break;
                        case MirAction.Die:
                            Effects.Add(new MirEffect(490, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 20, 40, Globals.LightningColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.UmaFlameThrower:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(520, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.FireColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.UmaKing:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(440, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 50, 80, Globals.LightningColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.ZumaKing:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(720, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.FireColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Show:
                            DXSoundManager.Play(SoundIndex.ZumaKingAppear);
                            break;
                    }
                    break;
                case MonsterImage.BanyaLeftGuard:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(100, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.FireColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Die:
                            Effects.Add(new MirEffect(200, 5, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.FireColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.BanyaRightGuard:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(0, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.LightningColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Die:
                            Effects.Add(new MirEffect(90, 5, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.LightningColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.EmperorSaWoo:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(510, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.FireColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.BoneArcher:
                case MonsterImage.BoneSoldier:
                case MonsterImage.BoneBladesman:
                    switch (action.Action)
                    {
                        case MirAction.Die:
                            Effects.Add(new MirEffect(630, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.BoneCaptain:
                    switch (action.Action)
                    {
                        case MirAction.Die:
                            Effects.Add(new MirEffect(650, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.ArchLichTaedu:
                    switch (action.Action)
                    {
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(1470, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Show:
                            Effects.Add(new MirEffect(1390, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Die:
                            Effects.Add(new MirEffect(1630, 17, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                                Skip = 20,
                            });
                            break;
                    }
                    break;
                case MonsterImage.RazorTusk:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(1800, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx, 20, 40, Globals.FireColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.Shinsu:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(980, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 20, 40, Globals.PhantomColour)
                            {
                                Target = this,
                                Blend = true,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(400),
                            });
                            break;
                    }
                    break;
                case MonsterImage.Stomper:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(1779, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.PachonTheChaosBringer:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(1800, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                        case MirAction.Die:
                            Effects.Add(new MirEffect(1890, 18, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagic, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.JinchonDevil:
                    switch (CurrentAction)
                    {
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(760, 9, TimeSpan.FromMilliseconds(70), LibraryFile.MonMagicEx2, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = Direction,
                            });
                            break;
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(990, 9, TimeSpan.FromMilliseconds(70), LibraryFile.MonMagicEx2, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = Direction,
                            });
                            break;
                    }
                    break;
                case MonsterImage.EmeraldDancer:
                    switch (CurrentAction)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(290, 20, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx5, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = Direction,
                                Skip = 20,
                            });
                            break;
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(540, 20, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx5, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            break;
                    }
                    break;
                case MonsterImage.FieryDancer:
                    switch (CurrentAction)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(570, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx5, 10, 35, Globals.FireColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            break;
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(620, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx5, 10, 35, Globals.FireColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            break;
                    }
                    break;
                case MonsterImage.QueenOfDawn:
                    switch (CurrentAction)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(680, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx5, 10, 35, Globals.HolyColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = Direction,
                            });
                            break;
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(460, 11, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx5, 30, 80, Globals.HolyColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            break;
                    }
                    break;
                case MonsterImage.OYoungBeast:
                    switch (CurrentAction)
                    {
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(600, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 0, 0, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = Direction
                            });
                            break;
                    }
                    break;
                case MonsterImage.MaWarlord:
                    switch (CurrentAction)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(1100, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 0, 0, Globals.NoneColour)
                            {
                                Blend = true,
                                Target = this,
                                Direction = Direction
                            });
                            break;
                    }
                    break;
                case MonsterImage.DragonQueen:
                    switch (CurrentAction)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(500, 20, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx6, 10, 35, Globals.DarkColour)
                            {
                                Blend = true,
                                Target = this,
                            });
                            break;
                    }
                    break;
                case MonsterImage.FerociousIceTiger:
                    switch (CurrentAction)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(700, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx7, 0, 0, Globals.NoneColour)
                            {
                                Blend = true,
                                MapTarget = Functions.Move(CurrentLocation, Direction, 3),
                                StartTime = CEnvir.Now.AddMilliseconds(600)
                            });
                            break;
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(801, 16, TimeSpan.FromMilliseconds(40), LibraryFile.MonMagicEx7, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            Effects.Add(new MirEffect(801, 16, TimeSpan.FromMilliseconds(40), LibraryFile.MonMagicEx7, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                                StartTime = CEnvir.Now.AddMilliseconds(150),
                            });
                            Effects.Add(new MirEffect(801, 16, TimeSpan.FromMilliseconds(40), LibraryFile.MonMagicEx7, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                                StartTime = CEnvir.Now.AddMilliseconds(300),
                            });
                            Effects.Add(new MirEffect(801, 16, TimeSpan.FromMilliseconds(40), LibraryFile.MonMagicEx7, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                                StartTime = CEnvir.Now.AddMilliseconds(450),
                            });
                            break;
                    }
                    break;
                case MonsterImage.NewMob1:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(1500, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 20, 40, Color.Purple)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(200),
                                Blend = true,
                            });
                            break;
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(1500, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 20, 50, Globals.IceColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(200),
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.MonasteryMon4:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(2600, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx23, 20, 40, Color.GreenYellow)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(200),
                                Blend = true,
                            });
                            break;
                        case MirAction.RangeAttack:
                            Effects.Add(new MirEffect(2600, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx23, 20, 50, Color.GreenYellow)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(200),
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.NewMob3:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(2700, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 20, 50, Globals.IceColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                StartTime = CEnvir.Now.AddMilliseconds(200),
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.NewMob10:
                    switch (action.Action)
                    {
                        case MirAction.Show:
                            Effects.Add(new MirEffect(3100, 18, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 20, 90, Color.Purple)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                                Skip = 0,
                            });
                            break;
                    }
                    break;
                case MonsterImage.NewMob6:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(2900, 6, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Direction = action.Direction,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.NewMob8:
                    switch (action.Action)
                    {
                        case MirAction.Show:
                            Effects.Add(new MirEffect(3220, 10, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(3200, 8, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
                case MonsterImage.NewMob4:
                case MonsterImage.NewMob5:
                    switch (action.Action)
                    {
                        case MirAction.Attack:
                            Effects.Add(new MirEffect(3200, 7, TimeSpan.FromMilliseconds(100), LibraryFile.MonMagicEx20, 0, 0, Globals.NoneColour)
                            {
                                Target = this,
                                Blend = true,
                            });
                            break;
                    }
                    break;
            }
        }

        public override void PlayAttackSound()
        {
            DXSoundManager.Play(AttackSound);
        }
        public override void PlayStruckSound()
        {
            DXSoundManager.Play(StruckSound);

            DXSoundManager.Play(SoundIndex.GenericStruckMonster);
        }
        public override void PlayDieSound()
        {
            DXSoundManager.Play(DieSound);
        }

        public override void UpdateQuests()
        {
            if (GameScene.Game.HasQuest(MonsterInfo, GameScene.Game.MapControl.MapInfo)) //Todo Optimize by variable.

                Title = "(任务)";
            else
                Title = string.Empty;
        }
    }
}

