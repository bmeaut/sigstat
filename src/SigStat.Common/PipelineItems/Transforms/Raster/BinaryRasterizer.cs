﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Drawing.Pens;
using SixLabors.Primitives;
using SigStat.Common.Helpers;
using SixLabors.Shapes;

namespace SigStat.Common.Transforms
{
    /// <summary>
    /// Converts standard features to a binary raster.
    /// <para>Default Pipeline Input: Standard <see cref="Features"/></para>
    /// <para>Default Pipeline Output: (bool[,]) Binarized</para>
    /// </summary>
    public class BinaryRasterizer : PipelineBase, ITransformation
    {
        private readonly int w;
        private readonly int h;
        private readonly float penWidth;
        private Byte4 fg = new Byte4(0, 0, 0, 255);
        private Byte4 bg = new Byte4(255, 255, 255, 255);
        private GraphicsOptions noAA = new GraphicsOptions(false);//aa kikapcs, mert binarisan dolgozunk es ne legyenek szakadasok
        private Pen<Byte4> pen;

        /// <summary> Initializes a new instance of the <see cref="BinaryRasterizer"/> class with specified raster size and pen width. </summary>
        /// <param name="resolutionX">Raster width.</param>
        /// <param name="resolutionY">Raster height.</param>
        /// <param name="penWidth"></param>
        public BinaryRasterizer(int resolutionX, int resolutionY, float penWidth)
        {
            this.w = resolutionX;
            this.h = resolutionY;
            this.penWidth = penWidth;
            pen = new Pen<Byte4>(fg, penWidth);
            this.Output(FeatureDescriptor<bool[,]>.Descriptor("Binarized"));
        }

        /// <inheritdoc/>
        public void Transform(Signature signature)
        {
            List<double> xs = signature.GetFeature(Features.X);
            List<double> ys = signature.GetFeature(Features.Y);
            List<bool> pendowns = signature.GetFeature(Features.Button);
            List<double> ps = signature.GetFeature(Features.Pressure);
            List<double> alts = signature.GetFeature(Features.Altitude);
            List<double> azs = signature.GetFeature(Features.Azimuth);
            //+ egyeb ami kellhet

            //TODO: X,Y legyen normalizalva, normalizaljuk ha nincs, ahhoz kell az Extrema, ..

            Image<Byte4> img = new Image<Byte4>(w, h);
            img.Mutate(ctx => ctx.Fill(bg));

            int len = xs.Count;
            List<PointF> points = new List<PointF>();
            for (int i=0;i<len;i++)
            {
                if (pendowns[i])
                {
                    points.Add(ToImageCoords(xs[i], ys[i]));
                }
                else
                {
                    if(points.Count>0)
                        DrawLines(img, points);
                    points = new List<PointF>();
                    points.Add(ToImageCoords(xs[i], ys[i]));
                }
                Progress = (int)(i / (double)len * 90);
            }
            DrawLines(img, points);

            bool[,] b = new bool[w, h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    b[x, y] = (img[x, y] == fg);

            signature.SetFeature(OutputFeatures[0], b);
            Progress = 100;
            Log(LogLevel.Info, "Rasterization done.");
        }

        private PointF ToImageCoords(double x, double y)
        {
            //ha x-et w-vel, y-t pedig h-val szoroznank, akkor torzulna
            //megtartjuk az aranyokat ugy, hogy w es h bol a kisebbiket valasztjuk (igy biztos belefer a kepbe minden)
            int m = Math.Min(w, h);

            int frame = m / 20;//keretet hagyunk, hogy ne a kep legszelerol induljon

            //Y-okat meg kell forditani, hogy ne legyen fejjel lefele a kepen
            //betesszuk a kep kozepere is
            return new PointF(
                (float)(frame + x * (m-frame*2) + (w-m)/2),
                (float)(frame + (1-y) * (m-frame*2) + (h-m)/2)
            );
        }

        private void DrawLines(Image<Byte4> img, List<PointF> ps)
        {
            img.Mutate(ctx => {
                ctx.DrawLines(noAA, pen, ps.ToArray());
                ctx.DrawLines(noAA, pen, ps.ToArray());// 2x kell meghivni hogy mukodjon??
            });
        }
    }
}
