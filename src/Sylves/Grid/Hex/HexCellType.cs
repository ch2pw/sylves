﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sylves
{
    public class HexCellType : ICellType
    {
        private static HexCellType ftInstance = new HexCellType(HexOrientation.FlatTopped);
        private static HexCellType ptInstance = new HexCellType(HexOrientation.PointyTopped);

        private readonly HexOrientation orientation;
        private readonly CellDir[] dirs;
        private readonly CellRotation[] rotations;
        private readonly CellRotation[] rotationsAndReflections;

        private HexCellType(HexOrientation orientation)
        {
            dirs = Enumerable.Range(0, 6).Select(x => (CellDir)x).ToArray();
            rotations = Enumerable.Range(0, 6).Select(x => (CellRotation)x).ToArray();
            rotationsAndReflections = rotations.Concat(Enumerable.Range(0, 6).Select(x => (CellRotation)~x)).ToArray();
            this.orientation = orientation;
        }

        public static HexCellType Get(HexOrientation orientation) => orientation == HexOrientation.FlatTopped ? ftInstance : ptInstance;

        public CellRotation ReflectY => orientation == HexOrientation.FlatTopped ? HexRotation.FTReflectY : HexRotation.PTReflectY;
        public CellRotation ReflectX => orientation == HexOrientation.FlatTopped ? HexRotation.FTReflectX : HexRotation.PTReflectX;

        public IEnumerable<CellDir> GetCellDirs()
        {
            return dirs;
        }

        public CellRotation GetIdentity()
        {
            return (CellRotation)0;
        }

        public Matrix4x4 GetMatrix(CellRotation cellRotation)
        {
            return ((HexRotation)cellRotation).ToMatrix(orientation);
        }

        public IList<CellRotation> GetRotations(bool includeReflections = false)
        {
            return includeReflections ? rotationsAndReflections : rotations;
        }

        public CellDir? Invert(CellDir dir)
        {
            return (CellDir)((3 + (int)dir) % 6);
        }

        public CellRotation Invert(CellRotation a)
        {
            if ((int)a < 0)
            {
                return a;
            }
            else
            {
                return (CellRotation)(6 - (int)a);
            }
        }

        public CellRotation Multiply(CellRotation a, CellRotation b)
        {
            var ia = (int)a;
            var ib = (int)b;
            if(ia >= 0)
            {
                if(ib >= 0)
                {
                    return (CellRotation)((ia + ib) % 6);
                }
                else
                {
                    return (CellRotation)~((6 + ia + ~ib) % 6);
                }
            }
            else
            {
                if (ib >= 0)
                {
                    return (CellRotation)~((6 + ~ia - ib) % 6);
                }
                else
                {
                    return (CellRotation)((6 + ~ia - ~ib) % 6);
                }
            }
        }

        public CellDir Rotate(CellDir dir, CellRotation rotation)
        {
            if((int)rotation >= 0)
            {
                return (CellDir)(((int)dir + (int)rotation) % 6);
            }
            else
            {
                return (CellDir)((6 - (int)dir + ~(int)rotation) % 6);
            }
        }

        public void Rotate(CellDir dir, CellRotation rotation, out CellDir resultDir, out Connection connection)
        {
            connection = new Connection {  Mirror = (int)rotation < 0 };
            resultDir = Rotate(dir, rotation);
        }
    }
}
