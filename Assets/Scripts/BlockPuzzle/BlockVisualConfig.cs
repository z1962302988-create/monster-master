using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterMaster.BlockPuzzle
{
    [Serializable]
    public sealed class BlockVisualConfig
    {
        [Serializable]
        public sealed class ExitSpriteOverride
        {
            public string exitId;
            public Sprite sprite;
        }

        [Header("棋盘底板")]
        public Sprite boardBackgroundSprite;

        [Header("通用形状（颜色专属图片留空时使用）")]
        public Sprite defaultBlockSprite;
        public Sprite defaultHorizontalTwoSprite;
        public Sprite defaultVerticalTwoSprite;
        public Sprite defaultLShapeSprite;
        public Sprite defaultSevenShapeSprite;
        public Sprite obstacleSprite;

        [Header("各颜色 · 单格方块")]
        public Sprite redSprite;
        public Sprite blueSprite;
        public Sprite greenSprite;
        public Sprite yellowSprite;
        public Sprite purpleSprite;
        public Sprite orangeSprite;
        public Sprite cyanSprite;
        public Sprite pinkSprite;
        public Sprite darkGreenSprite;
        public Sprite deepBlueSprite;

        [Header("各颜色 · 两格横形方块")]
        public Sprite redHorizontalTwoSprite;
        public Sprite blueHorizontalTwoSprite;
        public Sprite greenHorizontalTwoSprite;
        public Sprite yellowHorizontalTwoSprite;
        public Sprite purpleHorizontalTwoSprite;
        public Sprite orangeHorizontalTwoSprite;
        public Sprite cyanHorizontalTwoSprite;
        public Sprite pinkHorizontalTwoSprite;
        public Sprite darkGreenHorizontalTwoSprite;
        public Sprite deepBlueHorizontalTwoSprite;

        [Header("各颜色 · 两格竖形方块")]
        public Sprite redVerticalTwoSprite;
        public Sprite blueVerticalTwoSprite;
        public Sprite greenVerticalTwoSprite;
        public Sprite yellowVerticalTwoSprite;
        public Sprite purpleVerticalTwoSprite;
        public Sprite orangeVerticalTwoSprite;
        public Sprite cyanVerticalTwoSprite;
        public Sprite pinkVerticalTwoSprite;
        public Sprite darkGreenVerticalTwoSprite;
        public Sprite deepBlueVerticalTwoSprite;

        [Header("各颜色 · L 形方块")]
        public Sprite redLShapeSprite;
        public Sprite blueLShapeSprite;
        public Sprite greenLShapeSprite;
        public Sprite yellowLShapeSprite;
        public Sprite purpleLShapeSprite;
        public Sprite orangeLShapeSprite;
        public Sprite cyanLShapeSprite;
        public Sprite pinkLShapeSprite;
        public Sprite darkGreenLShapeSprite;
        public Sprite deepBlueLShapeSprite;

        [Header("各颜色 · 7 形方块")]
        public Sprite redSevenShapeSprite;
        public Sprite blueSevenShapeSprite;
        public Sprite greenSevenShapeSprite;
        public Sprite yellowSevenShapeSprite;
        public Sprite purpleSevenShapeSprite;
        public Sprite orangeSevenShapeSprite;
        public Sprite cyanSevenShapeSprite;
        public Sprite pinkSevenShapeSprite;
        public Sprite darkGreenSevenShapeSprite;
        public Sprite deepBlueSevenShapeSprite;

        [Header("状态图标")]
        public Sprite keyIcon;
        public Sprite lockIcon;

        [Header("出口（单独覆盖优先于方向图片）")]
        public Sprite defaultExitSprite;
        public Sprite leftExitSprite;
        public Sprite rightExitSprite;
        public Sprite bottomExitSprite;
        public Sprite topExitSprite;
        public List<ExitSpriteOverride> exitOverrides = new List<ExitSpriteOverride>();

        public Sprite GetBlockSprite(BlockColorType color)
        {
            Sprite sprite;
            switch (color)
            {
                case BlockColorType.Red: sprite = redSprite; break;
                case BlockColorType.Blue: sprite = blueSprite; break;
                case BlockColorType.Green: sprite = greenSprite; break;
                case BlockColorType.Yellow: sprite = yellowSprite; break;
                case BlockColorType.Purple: sprite = purpleSprite; break;
                case BlockColorType.Orange: sprite = orangeSprite; break;
                case BlockColorType.Cyan: sprite = cyanSprite; break;
                case BlockColorType.Pink: sprite = pinkSprite; break;
                case BlockColorType.DarkGreen: sprite = darkGreenSprite; break;
                case BlockColorType.DeepBlue: sprite = deepBlueSprite; break;
                default: sprite = null; break;
            }
            return sprite != null ? sprite : defaultBlockSprite;
        }

        public Sprite GetShapeSprite(BlockColorType color, BlockShape shape)
        {
            if (shape != BlockShape.LShape && shape != BlockShape.SevenShape)
                return null;

            Sprite sprite;
            bool isLShape = shape == BlockShape.LShape;
            switch (color)
            {
                case BlockColorType.Red: sprite = isLShape ? redLShapeSprite : redSevenShapeSprite; break;
                case BlockColorType.Blue: sprite = isLShape ? blueLShapeSprite : blueSevenShapeSprite; break;
                case BlockColorType.Green: sprite = isLShape ? greenLShapeSprite : greenSevenShapeSprite; break;
                case BlockColorType.Yellow: sprite = isLShape ? yellowLShapeSprite : yellowSevenShapeSprite; break;
                case BlockColorType.Purple: sprite = isLShape ? purpleLShapeSprite : purpleSevenShapeSprite; break;
                case BlockColorType.Orange: sprite = isLShape ? orangeLShapeSprite : orangeSevenShapeSprite; break;
                case BlockColorType.Cyan: sprite = isLShape ? cyanLShapeSprite : cyanSevenShapeSprite; break;
                case BlockColorType.Pink: sprite = isLShape ? pinkLShapeSprite : pinkSevenShapeSprite; break;
                case BlockColorType.DarkGreen: sprite = isLShape ? darkGreenLShapeSprite : darkGreenSevenShapeSprite; break;
                case BlockColorType.DeepBlue: sprite = isLShape ? deepBlueLShapeSprite : deepBlueSevenShapeSprite; break;
                default: sprite = null; break;
            }

            if (sprite != null) return sprite;
            return isLShape ? defaultLShapeSprite : defaultSevenShapeSprite;
        }

        public Sprite GetRectangleSprite(BlockColorType color, int width, int height)
        {
            bool horizontalTwo = width == 2 && height == 1;
            bool verticalTwo = width == 1 && height == 2;
            if (!horizontalTwo && !verticalTwo)
                return GetBlockSprite(color);

            Sprite sprite;
            switch (color)
            {
                case BlockColorType.Red: sprite = horizontalTwo ? redHorizontalTwoSprite : redVerticalTwoSprite; break;
                case BlockColorType.Blue: sprite = horizontalTwo ? blueHorizontalTwoSprite : blueVerticalTwoSprite; break;
                case BlockColorType.Green: sprite = horizontalTwo ? greenHorizontalTwoSprite : greenVerticalTwoSprite; break;
                case BlockColorType.Yellow: sprite = horizontalTwo ? yellowHorizontalTwoSprite : yellowVerticalTwoSprite; break;
                case BlockColorType.Purple: sprite = horizontalTwo ? purpleHorizontalTwoSprite : purpleVerticalTwoSprite; break;
                case BlockColorType.Orange: sprite = horizontalTwo ? orangeHorizontalTwoSprite : orangeVerticalTwoSprite; break;
                case BlockColorType.Cyan: sprite = horizontalTwo ? cyanHorizontalTwoSprite : cyanVerticalTwoSprite; break;
                case BlockColorType.Pink: sprite = horizontalTwo ? pinkHorizontalTwoSprite : pinkVerticalTwoSprite; break;
                case BlockColorType.DarkGreen: sprite = horizontalTwo ? darkGreenHorizontalTwoSprite : darkGreenVerticalTwoSprite; break;
                case BlockColorType.DeepBlue: sprite = horizontalTwo ? deepBlueHorizontalTwoSprite : deepBlueVerticalTwoSprite; break;
                default: sprite = null; break;
            }
            if (sprite != null) return sprite;

            Sprite shapeDefault = horizontalTwo ? defaultHorizontalTwoSprite : defaultVerticalTwoSprite;
            return shapeDefault != null ? shapeDefault : GetBlockSprite(color);
        }

        public Sprite GetExitSprite(ExitData exit)
        {
            foreach (ExitSpriteOverride item in exitOverrides)
            {
                if (item != null && item.exitId == exit.id && item.sprite != null)
                    return item.sprite;
            }

            Sprite directionSprite;
            switch (exit.edge)
            {
                case BoardEdge.Left: directionSprite = leftExitSprite; break;
                case BoardEdge.Right: directionSprite = rightExitSprite; break;
                case BoardEdge.Bottom: directionSprite = bottomExitSprite; break;
                default: directionSprite = topExitSprite; break;
            }
            return directionSprite != null ? directionSprite : defaultExitSprite;
        }
    }
}
