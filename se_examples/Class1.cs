using System;
using System.Collections.Generic;

//! netstandard.dll
//! System.dll
//! System.Core.dll
//! System.Data.dll
//! System.Linq.dll
//! System.Xml.dll

//! Sandbox.Common.dll
//! Sandbox.Game.dll

//! VRage.dll
//! VRage.Game.dll
//! VRage.Math.dll
//! VRage.Library.dll

namespace ExtRenderer {
    class Program {
        public Program(Queue<string> blub) {
            this.log = blub;
        }

        private readonly Queue<string> log;
        public void WriteLine(string xx) {
            this.log.Enqueue(string.Format("[{0}]: {1}", DateTime.Now, xx));
        }


        HashSet<VRage.ModAPI.IMyEntity> entities = null;
        List<VRage.Game.ModAPI.IMySlimBlock> blocks = null;
        List<VRage.Game.ModAPI.IMySlimBlock> inventory_blocks = null;
        VRage.Game.ModAPI.IMyInventory main_cargo = null;
        VRage.Game.ModAPI.IMyCubeGrid grid = null;

        private string SUPPLY_CARGO_NAME = "[Supply Cargo]";
        private string SUPPLY_CARGO_GRID_NAME = "FUK.Bumblebee";

        private string PROJ_GRID_NAME = "Main Base";

        private bool RefreshEntities() {
            if (ReferenceEquals(entities, null)) entities = new HashSet<VRage.ModAPI.IMyEntity>();
            else entities.Clear();

            Sandbox.ModAPI.MyAPIGateway.Entities.GetEntities(entities, e => e != null && e as VRage.Game.ModAPI.Ingame.IMyCubeGrid != null);

            //get self
            VRage.Game.ModAPI.Ingame.IMyEntity container_entity = null;
            VRage.Game.ModAPI.Ingame.IMyEntity grid_entity = null;

            foreach (var e in entities) {
                if (e.DisplayName == SUPPLY_CARGO_GRID_NAME) {
                    container_entity = e;
                }

                if (e.DisplayName == PROJ_GRID_NAME) {
                    grid_entity = e;
                }

            }

            if (ReferenceEquals(container_entity, null)) {
                this.WriteLine("container_entity is null");
                return false;
            }

            grid = (VRage.Game.ModAPI.IMyCubeGrid)grid_entity;
            if (ReferenceEquals(blocks, null)) blocks = new List<VRage.Game.ModAPI.IMySlimBlock>();
            else blocks.Clear();

            grid.GetBlocks(blocks);

            grid = (VRage.Game.ModAPI.IMyCubeGrid)container_entity;
            if (ReferenceEquals(inventory_blocks, null)) inventory_blocks = new List<VRage.Game.ModAPI.IMySlimBlock>();
            else inventory_blocks.Clear();
            grid.GetBlocks(inventory_blocks, x => x.FatBlock != null && x.FatBlock.HasInventory);

            foreach (var cargo in inventory_blocks) {
                if (cargo.FatBlock.DisplayNameText == SUPPLY_CARGO_NAME) {
                    main_cargo = cargo.FatBlock.GetInventory(0);
                    break;
                }
            }

            if (ReferenceEquals(main_cargo, null)) {
                this.WriteLine("no supplier cargo");
                return false;
            }

            return true;
        }

        public void Main() {
            if (!RefreshEntities()) return;

            var selfEntity = Sandbox.Game.World.MySession.Static.LocalCharacter.Entity;

            foreach (var block in blocks) {
                if (block.BuildLevelRatio < 1) {
                    var slimBlock = (Sandbox.Game.Entities.Cube.MySlimBlock)block;
                    var hcargo = (Sandbox.Game.MyInventory)main_cargo;
                    Sandbox.Game.Entities.Cube.SeBridge.RFS(slimBlock, hcargo);
                }
            }

        }
    }
}

namespace Sandbox.Game.Entities.Cube {
    class SeBridge {
        public static void RFS(MySlimBlock block, MyInventory inventory) {
            block.CubeGrid.RequestFillStockpile(block.Position, inventory);
        }
    }
}