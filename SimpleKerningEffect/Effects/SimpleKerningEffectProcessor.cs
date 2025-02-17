﻿using System.Text.RegularExpressions;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Player.Video;

namespace SimpleKerningEffect.Effects
{
    internal class SimpleKerningEffectProcessor : IVideoEffectProcessor
    {
        readonly SimpleKerningEffect item;
        ID2D1Image? input;

        public ID2D1Image Output => input ?? throw new NullReferenceException(nameof(input) + "is null");

        public SimpleKerningEffectProcessor(SimpleKerningEffect item)
        {
            this.item = item;
        }

        public DrawDescription Update(EffectDescription effectDescription)
        {
            var drawDesc = effectDescription.DrawDescription;
            var inputIndex = effectDescription.InputIndex + 1;
            
            var index = item.Index.Replace(" ", "");
            var part = index.Split(",");
            var match = false;
            for (int i = 0; i < part.Length; i++)
            {
                var numerics = Regex.IsMatch(part[i], @"^[0-9]{1,9}$");
                var hyphen = Regex.IsMatch(part[i], @"^[0-9]{1,9}-[0-9]{1,9}$");
                if (!numerics && !hyphen) return drawDesc;
                if (numerics)
                    match = (int.Parse(part[i]) == inputIndex) ? true : match;
                if (hyphen)
                {
                    var piece = part[i].Split("-");
                    var first = int.Parse(piece[0]);
                    var second = int.Parse(piece[1]);
                    if (first > second) return drawDesc;
                    match = (first <= inputIndex) && (inputIndex <= second) ? true : match;                  
                }
            }
            if (!match) return drawDesc;

            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;

            var x = item.X.GetValue(frame, length, fps);
            var y = item.Y.GetValue(frame, length, fps);
            var z = item.Z.GetValue(frame, length, fps);
            var opacity = item.Opacity.GetValue(frame, length, fps) / 100;
            var invert = item.Invert;
            var zoom = item.Zoom.GetValue(frame, length, fps) / 100;
            var zoomX = zoom * item.ZoomX.GetValue(frame, length, fps) / 100;
            var zoomY = zoom * item.ZoomY.GetValue(frame, length, fps) / 100;
            var rotationX = item.RotationX.GetValue(frame, length, fps);
            var rotationY = item.RotationY.GetValue(frame, length, fps);
            var rotationZ = item.RotationZ.GetValue(frame, length, fps);

            return
                drawDesc with
                {
                    Draw = new(
                    drawDesc.Draw.X + (float)x,
                    drawDesc.Draw.Y + (float)y,
                    drawDesc.Draw.Z + (float)z),
                    Opacity = drawDesc.Opacity * opacity,
                    Zoom = new(
                    drawDesc.Zoom.X * (float)zoomX,
                    drawDesc.Zoom.Y * (float)zoomY),
                    Rotation = new(
                    drawDesc.Rotation.X + (float)rotationX,
                    drawDesc.Rotation.Y + (float)rotationY,
                    drawDesc.Rotation.Z + (float)rotationZ),
                    Invert = invert ? !drawDesc.Invert : drawDesc.Invert
                };
        }
        public void ClearInput()
        {
            input = null;
        }
        public void SetInput(ID2D1Image? input)
        {
            this.input = input;
        }

        public void Dispose()
        {

        }

    }
}