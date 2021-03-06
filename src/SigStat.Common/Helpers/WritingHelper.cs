//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Drawing;
//using System.Linq;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.PixelFormats;

//namespace SigStat.Common
//{
//    public static class WritingHelper
//    {

//		public static readonly object SyncRoot = new object();

//        public static bool ValidPixel(int i, int j, int height, int width)
//        {
//            return i >= 0 && j >= 0 && i < height && j < width;
//        }

//        private static Point GetNext(byte[,] img, Point current, Point previous, Point[] ignorePoints)
//        {
//			Point result = new Point(-1,-1);
//            foreach (Point p in Matrix.GetNeighbourPixels(current))
//            {
//                bool ignore = false;
//                if (p.X < 0 || p.Y < 0 || p.X >= img.GetLength(0) || p.Y >= img.GetLength(1)) ignore = true;
//                foreach (Point ip in ignorePoints)
//                    if (p.X == ip.X && p.Y == ip.Y) ignore = true;
//                if (p.X == previous.X && p.Y == previous.Y) ignore = true;
//                if (!ignore && img[p.X, p.Y] > 0)
//                {
//                    if (result.X == -1)
//                        result = p;
//                    else
//                        //B gáz van, egynél több úton mehetnénk tovább
//                        return new Point(-1, -1); ;
//                }
//            }
//            return result;
//        }

//        private static Point GetNext(byte[,] img, Point current, Point previous)
//        {
//            foreach (Point p in Matrix.GetNeighbourPixels(current))
//            {
//                if (img[p.X, p.Y] > 0 && !(p.X == previous.X && p.Y == previous.Y)) return p;
//            }
//            return new Point(-1, -1);
//        }

//        public static List<Point> Follow(byte[,] img, byte[,] neighbours, Point startPoint)
//        {
//            return Follow(img, neighbours, new List<Point>(new Point[] { startPoint }), new Point[] { }, int.MaxValue);
//        }

//		/// <summary>
//		/// Egy bináris kép egy adott pontjából elindulva, sorban végigjárja a szomszédos pixeleket, de legfeljebb maxCount darabot.
//		/// Ha olyan pixelhez ér, amelynek egynél több új szomszédja van, akkor a követés leáll.
//		/// </summary>
//		/// <param name="img">a szürkeárnyalatos kép, amin a 0-nál nagyobb intenzitású pixeleket keressük</param>
//		/// <param name="neighbours">???</param>
//		/// <param name="existingPoints">a korábban már megtalált pontok (alapesetben a követés kiindulópontját kell megadni)</param>
//		/// <param name="ignorePoints">olyan pontok, amiket nem kell figyelembe venni</param>
//		/// <param name="maxCount">legfeljebb ennyi pixelen keresztül követjük a görbét</param>
//		/// <returns></returns>
//        public static List<Point> Follow(byte[,] img, byte[,] neighbours, IEnumerable<Point> existingPoints, Point[] ignorePoints, int maxCount)
//        {
//            List<Point> points = new List<Point>(existingPoints);

//            Point current = points[points.Count - 1];
//            Point previous = new Point(-1, -1);
//            if (points.Count > 1) previous = points[points.Count - 2];
//			// addig megyünk, amíg elágazást nem találunk, de max maxCount pontig, vagy a vonal végéig
//			while (points.Count < maxCount)
//            {
//                points.Add(current);
//                Point p = GetNext(img, current, previous, ignorePoints);
//				if (p.X == -1)
//					break;

//                previous = current;
//                current = p;
//            }
//            return points;
//        }

//        /// <summary>
//        /// Overload, aminek segítségével bool-tömbbel is használható a Follow függvény
//        /// </summary>
//        /// <returns></returns>
//        public static List<Point> Follow(bool[,] img, byte[,] neighbours, IEnumerable<Point> existingPoints, Point[] ignorePoints, int maxCount)
//        {
//            byte[,] byteImg = new byte[img.GetLength(0), img.GetLength(1)];

//            for (int i = 0; i < img.GetLength(0); i++)
//            {
//                for (int j = 0; j < img.GetLength(1); j++)
//                {
//                    byteImg[i, j] = img[i, j] ? (byte)1 : (byte)0;
//                }
//            }

//            return Follow(byteImg, neighbours, existingPoints, ignorePoints, maxCount);
//        }

//        public static List<Point> Follow(byte[,] rawImage, Point source, Nullable<Point> ignitionPoint, int maxStep)
//        {
//            List<Point> result = new List<Point>();

//            Point previous = new Point(-1, -1);

//            if (ignitionPoint.HasValue)
//            {
//                previous = ignitionPoint.Value;
//            }

//            bool isEnded = false;

//            for (int step = 0; !isEnded && (maxStep == -1 || step < maxStep); step++)
//            {
//                result.Add(source);

//                var neighbours = source.GetNeighbourPixels();
//                Point next = new Point(-1, -1);

//                foreach (var neighbour in neighbours)
//                {
//                    if (ValidPixel(neighbour.X, neighbour.Y, rawImage.GetLength(0), rawImage.GetLength(1)) &&
//                        neighbour != previous && rawImage[neighbour.X, neighbour.Y] > 0)
//                    {
//                        if (next.X == -1)
//                        {
//                            next = neighbour;
//                        }
//                        else
//                        {
//                            isEnded = true;

//                            break;
//                        }
//                    }
//                }

//                if (next.X == -1)
//                {
//                    isEnded = true;
//                }
//                else
//                {
//                    previous = source;
//                    source = next;
//                }
//            }

//            return result;
//        }

//        public static List<Point> Follow(byte[,] rawImage, Point source, Nullable<Point> ignitionPoint)
//        {
//            return Follow(rawImage, source, ignitionPoint, -1);
//        }

//        public static bool HasOneNeighbour(byte[,] rawImage, Point point)
//        {
//            bool result = false;

//            foreach (Point pt in point.GetNeighbourPixels())
//            {
//                if (ValidPixel(pt.X, pt.Y, rawImage.GetLength(0), rawImage.GetLength(1)) && rawImage[pt.X, pt.Y] > 0)
//                {
//                    if (!result)
//                        result = true;
//                    else
//                        return false;
//                }
//            }

//            return result;
//        }

//        public static Vector GetDirection(List<Point> points)
//        {
//            int sumdx = 0; int sumdy = 0;
//            int x = points[0].X; int y = points[0].Y;
//            foreach (Point p in points)
//            {
//                sumdx += p.X - x;
//                sumdy += p.Y - y;
//            }
//            int dx = Math.Abs(sumdx);
//            int dy = Math.Abs(sumdy);

//            // normalize results
//            if (dx > dy)
//            {
//                dy = Math.Sign(sumdy) * dy * 10 / dx;
//                dx = Math.Sign(sumdx) * 10;
//            }
//            else if (dy > dx)
//            {
//                dx = Math.Sign(sumdx) * dx * 10 / dy;
//                dy = Math.Sign(sumdy) * 10;
//            }
//            else // dx = dy
//            {
//                dx = Math.Sign(sumdx) * 10;
//                dy = Math.Sign(sumdy) * 10;
//            }
//            return new Vector(x, y, dx, dy);
//        }

//        public static bool Connected4(Point p1, Point p2)
//        {
//            return (Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y)) == 1;
//        }

//        public static bool Connected8(Point p1, Point p2)
//        {
//            return Math.Abs(p1.X - p2.X)<2 && Math.Abs(p1.Y - p2.Y)<2;
//        }

//        public static bool LooslyConnected(Point p1, Point p2)
//        {
//            return Math.Abs(p1.X - p2.X) != 2 && Math.Abs(p1.Y - p2.Y) != 2;
//        }

//        internal static bool CanRemove(byte[,] img, Point p)
//        {
//            List<Point> nbList =
//                new List<Point>(
//                    Matrix.GetNeighbourPixels(p)   // Get alll neighbours
//                    ).FindAll(delegate(Point nb) { return (img[nb.X, nb.Y] > 0); }); // And extract those with real points

//            if (nbList.Count == 0) return true;

//            return (GetConnectedPoints(nbList).Count == nbList.Count);
//        }

//        private static List<Point> GetConnectedPoints(List<Point> points)
//        {
//            List<Point> source = new List<Point>(points);
//            List<Point> connList = new List<Point>();
//            List<Point> connTmpList = new List<Point>();

//            connList.Add(source[0]);
//            source.Remove(source[0]);

//            bool changed = true;

//            while (changed)
//            {
//                changed = false;
//                connTmpList.Clear();
//                foreach (Point p1 in connList)
//                    foreach (Point p2 in source)
//                        if (p1 != p2 && LooslyConnected(p1, p2))
//                            connTmpList.Add(p2);
//                while (connTmpList.Count > 0)
//                {
//                    connList.Add(connTmpList[0]);
//                    source.Remove(connTmpList[0]);
//                    connTmpList.Remove(connTmpList[0]);
//                    changed = true;
//                }
//            }
//            return connList;
//        }


//        /// <summary>
//        /// Visszaad egy StrokePoint tömböt, melynek első és utolsó pontja azonos pozíciójú a két
//        /// paraméterként megadottal, s közbülső elemei sorban a startPoint-tól a targetPoint-ig 
//        /// vezető út pontjait tartalmazzák
//        /// </summary>
//        /// <param name="startPoint"></param>
//        /// <param name="targetPoint"></param>
//        /// <returns></returns>
//        public static IEnumerable<Point> GetPath(Point startPoint, Point targetPoint)
//        {

//            int dx = targetPoint.X - startPoint.X;
//            int dy = targetPoint.Y - startPoint.Y;
//            int adx = Math.Abs(dx);
//            int ady = Math.Abs(dy);

//            if (adx == 0 && ady == 0)
//            {
//                return new Point[1] { new Point(startPoint.X, startPoint.Y) };
//            }
//            // Függőleges vonalak
//            if (adx == 0)
//            {
//                Point[] points = new Point[ady + 1];
//                int sdy = Math.Sign(dy);
//                for (int y = startPoint.Y; y != targetPoint.Y + sdy; y += sdy)
//                    points[(y - startPoint.Y) * sdy] = new Point(startPoint.X, y);
//                return points;
//            }
//            // Vízszintes vonalak
//            if (ady == 0)
//            {
//                Point[] points = new Point[adx + 1];
//                int sdx = Math.Sign(dx);
//                for (int x = startPoint.X; x != targetPoint.X + sdx; x += sdx)
//                    points[(x - startPoint.X) * sdx] = new Point(x, startPoint.Y);
//                return points;
//            }
//            // Ha a vízszintes vetület a hosszabb, akkor minden vízszintes x koordinátához egy y-t keresünk...
//            if (adx > ady)
//            {
//                Point[] points = new Point[adx + 1];
//                int sdx = Math.Sign(dx);
//                for (int n = 0; n <= adx; n += 1)
//                    points[n] = new Point(startPoint.X + n * sdx, startPoint.Y + n * dy / adx);
//                return points;
//            }
//            else
//            // ...ellenkező esetben minden függőleges y koordinátához egy x-et keresünk
//            {
//                Point[] points = new Point[ady + 1];
//                int sdy = Math.Sign(dy);
//                for (int n = 0; n <= ady; n += 1)
//                    points[n] = new Point(startPoint.X + n * dx / ady, startPoint.Y + n * sdy);
//                return points;
//            }

//        }

//        private static Dictionary<string, List<Point>> cache = new Dictionary<string, List<Point>>();
//        public static List<Point> GetDiskPoints(Point p, int radius)
//        {
//            // TODO: optimalizálni
//            lock (SyncRoot)
//            {
//                string key = "disk " + radius;
//                List<Point> result;
//                if (!cache.TryGetValue(key, out result))
//                {
//                    result = new List<Point>();

//                    for (int y = -radius; y <= +radius; y++)
//                    {
//                        int dx = Convert.ToInt32(Math.Sqrt((double)radius * radius - y * y));
//                        for (int x = -dx; x <= dx; x++)
//                            result.Add(new Point(x, y));
//                    }
//                    cache.Add(key, result);
//                }
//                return MoveAndCopy(result, p.X, p.Y);
//            }
//        }


//        public static IEnumerable<Point> GetRectanglePoints(int left, int top, int width, int height)
//        {
//            // Ez nem számításigényes, ezért nincs szükség Cache-elésre
//            for (int y = top; y < top + height; y++)
//                for (int x = left; x < left + width; x++)
//                {
//                    yield return new Point(x, y);
//                }
//        }


//        public static List<Point> GetCirclePoints(Point p, int radius)
//        {
//            lock (SyncRoot)
//            {
//                // Hatékony körrajzoló algoritmus
//                // Szükség esetén egy picikét mégtovább lehet tuningolni: http://www.cs.unc.edu/~mcmillan/comp136/Lecture7/circle.html
//                string key = "circle " + radius;
//                List<Point> result;
//                if (!cache.TryGetValue(key, out result))
//                {
//                    List<Point> q1 = new List<Point>();
//                    List<Point> q2 = new List<Point>();
//                    List<Point> q3 = new List<Point>();
//                    List<Point> q4 = new List<Point>();
//                    List<Point> q5 = new List<Point>();
//                    List<Point> q6 = new List<Point>();
//                    List<Point> q7 = new List<Point>();
//                    List<Point> q8 = new List<Point>();

//                    int r2 = radius * radius;

//                    int y = radius;
//                    int x = 0;
//                    //y = (int)(Math.sqrt(r2 - 1) + 0.5);
//                    while (x <= y)
//                    {
//                        q4.Add(new Point(+x, +y));
//                        q1.Add(new Point(+x, -y));
//                        q5.Add(new Point(-x, +y));
//                        q8.Add(new Point(-x, -y));
//                        q3.Add(new Point(+y, +x));
//                        q2.Add(new Point(+y, -x));
//                        q6.Add(new Point(-y, +x));
//                        q7.Add(new Point(-y, -x));

//                        x += 1;
//                        y = (int)(Math.Sqrt(r2 - x * x) + 0.5);
//                    }
//                    q2.Reverse();
//                    q4.Reverse();
//                    q6.Reverse();
//                    q8.Reverse();

//                    result = new List<Point>();
//                    result.AddRange(q1);
//                    result.AddRange(q2);
//                    result.AddRange(q3);
//                    result.AddRange(q4);
//                    result.AddRange(q5);
//                    result.AddRange(q6);
//                    result.AddRange(q7);
//                    result.AddRange(q8);

//                    // Az érintkezéseknél lehetnek duplikációk
//                    int i = 0;
//                    while (i < result.Count - 1)
//                    {
//                        if (result[i] == result[i + 1])
//                            result.RemoveAt(i);
//                        else
//                            i++;
//                    }
//                    cache.Add(key, result);
//                }
//                return MoveAndCopy(result, p.X, p.Y);
//            }
//        }


//        public static List<Point> MoveAndCopy(List<Point> points, int dx, int dy)
//        {
//            //KG: kb 20%-kal gyorsabb a transzformáció ha a lista méretét előre megadjuk
//            Point[] result = new Point[points.Count];

//            for (int i = 0; i < points.Count; i++)
//                result[i] = new Point(points[i].X + dx, points[i].Y + dy);

//            return new List<Point>(result);
//        }

//        public static void Move(Point[] points, int dx, int dy)
//        {
//            for (int i = 0; i < points.Length; i++)
//            {
//                points[i].X += dx;
//                points[i].Y += dy;
//            }
//        }

      
//        public static void Move(Point point, int dx, int dy)
//        {
//            point.X += dx;
//            point.Y += dy;
//        }




//        /// <summary>
//        /// Visszaadja adott pont intenzitását megfelelő színsúlyozás mellett
//        /// </summary>
//        /// <param name="p"></param>
//        /// <param name="img"></param>
//        /// <returns></returns>
//        public static int GetIntensity(this Point p, Image<Rgba32> img)
//        {
//            int red = img[p.X, p.Y].R;
//            int green = img[p.X, p.Y].G;
//            int blue = img[p.X, p.Y].B;
//            int intensity = (int)(blue - (red + green) / 2);
//            return intensity;
//        }



//        /// <summary>
//        /// Két pont távolságának négyzetét adja vissza
//        /// </summary>
//        /// <param name="a"></param>
//        /// <param name="b"></param>
//        /// <returns></returns>
//        public static double DistanceSquare(Point a, Point b)
//        {
//            return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
//        }



//        /// <summary>
//        /// Két pont távolságát adja vissza
//        /// </summary>
//        /// <param name="a"></param>
//        /// <param name="b"></param>
//        /// <returns></returns>
//        public static double Distance(Point a, Point b)
//        {
//            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
//        }

//        /// <summary>
//        /// Két pont távolságát adja vissza
//        /// </summary>
//        /// <returns></returns>
//        public static double Distance(int x1, int y1, int x2, int y2)
//        {
//            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
//        }

//        /// <summary>
//        /// Két pont távolságát adja vissza
//        /// </summary>
//        /// <returns></returns>
//        public static double Distance(double x1, double y1, double x2, double y2)
//        {
//            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
//        }


//        /// <summary>
//        /// Igaz, ha a két pont távolsága kisebb a megadott értéknél
//        /// </summary>
//        /// <param name="a"></param>
//        /// <param name="b"></param>
//        /// <returns></returns>
//        public static bool DistanceSmallerThan(int x1, int y1, int x2, int y2, int distance)
//        {
//            return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) < (distance * distance);
//        }

//        /// <summary>
//        /// Két pont Manhattan távolságát adja vissza
//        /// </summary>
//        /// <param name="a"></param>
//        /// <param name="b"></param>
//        /// <returns></returns>
//        public static int ManhattanDistance(Point a, Point b)
//        {
//            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
//        }




//        /// <summary>
//        /// Két szakasz metszéspontját adja vissza pontos (float) koordinátákkal
//        /// </summary>
//        /// <param name="v1"></param>
//        /// <param name="v2"></param>
//        /// <returns></returns>
//        public static PointF IntersectF(VectorF v1, VectorF v2)
//        {
//            double x = 0;
//            double y = 0;


//            // Ha mindkétszakasz lejt, akkor minden mehet simán
//            if (v1.Vx != 0 && v2.Vx != 0)
//            {
//                x = (v2.B - v1.B) / (v1.M - v2.M);
//                y = x * v1.M + v1.B;
//            }
//            // Ha csak az első szakasz függőleges, akkor x koordináta adott, és a második szakasz egyenletébe helyettesítjük be
//            else if (v1.Vx == 0)
//            {
//                x = v1.X;
//                y = x * v2.M + v2.B;
//            }
//            // Ha csak a második szakasz függőleges, akkor x koordináta adott, és az első szakasz egyenletébe helyettesítjük be
//            else if (v2.Vx == 0)
//            {
//                x = v2.X;
//                y = x * v1.M + v1.B;
//            }
//            else
//                throw new Exception("Két függőleges szakasznak nem meghatározható a metszéspontja");


//            return new PointF((float)x, (float)y);
//        }


//        /// <summary>
//        /// Pont és szakasz távolságát adja vissza
//        /// </summary>
//        /// <param name="v1"></param>
//        /// <param name="p"></param>
//        /// <returns></returns>
//        public static double Distance(Vector v1, Point p)
//        {
//            // megkeressük a pontból a szakasz egyenesére állított merőleges metszéspontját
//            Vector diagonal = v1.GetNormal();
//            diagonal.X = p.X;
//            diagonal.Y = p.Y;
//            PointF intersect = IntersectF(v1, diagonal);

//            // Ha intersect a szakaszon belül van, akkor a merőleges szakasz hossza lesz a távolság
//            if (intersect.X.Between(v1.X, v1.X2) && intersect.Y.Between(v1.Y, v1.Y2))
//                return Distance(intersect.X, intersect.Y, p.X, p.Y);
//            // ellenkező esetben a legközelebbi szakaszvégtől mért távolságot adjuk vissza
//            else
//                return Math.Min(Distance(v1.X, v1.Y, p.X, p.Y), Distance(v1.X2, v1.Y2, p.X, p.Y));
//        }


//        /// <summary>
//        /// Két szakasz távolságát adja meg. Egymást metsző/érintő szakaszok esetén a távolság 0
//        /// </summary>
//        /// <param name="v1"></param>
//        /// <param name="p"></param>
//        /// <returns></returns>
//        public static double Distance(Vector v1, Vector v2)
//        {
//            PointF intersect = IntersectF(v1, v2);

//            // Ha a metszéspont minden vektoron belül van, akor a két vektor metszi egymást
//            if (intersect.X.Between(v1.X, v1.X2) && intersect.Y.Between(v1.Y, v1.Y2) &&
//                intersect.X.Between(v2.X, v2.X2) && intersect.Y.Between(v2.Y, v2.Y2))
//                return 0;

//            // Ellenkező esetben megkeressük a másik vektorhoz legközelebb eső végpontot
//            double d1 = Distance(v1, v2.Start);
//            double d2 = Distance(v1, v2.End);
//            double d3 = Distance(v2, v1.Start);
//            double d4 = Distance(v2, v1.End);

//            return new[] { d1, d2, d3, d4 }.Min();
//        }


//        /// <summary>
//        /// Pont és törtvonal távolságát adja vissza
//        /// </summary>
//        /// <param name="vectors"></param>
//        /// <param name="p"></param>
//        /// <returns></returns>
//        public static double Distance(IEnumerable<Vector> vectors, Point p)
//        {
//            return vectors.Select(v => Distance(v, p)).Min();
//        }


//        /// <summary>
//        /// Egy adott középpontú és sugarú kör határoló téglalapját adja vissza
//        /// </summary>
//        /// <param name="strokePoint"></param>
//        /// <param name="scannerRadius"></param>
//        /// <returns></returns>
//        public static Rectangle GetCircleBounds(Point center, int radius)
//        {
//            return new Rectangle(center.X - radius, center.Y - radius, radius * 2 + 1, radius * 2 + 1);
//        }

//        /// <summary>
//        /// P0, P1, P2 háromszög P0 pontjában lévő szögét adja vissza fokban [0,180]
//        /// </summary>
//        /// <param name="x0"></param>
//        /// <param name="y0"></param>
//        /// <param name="x1"></param>
//        /// <param name="y1"></param>
//        /// <param name="x2"></param>
//        /// <param name="y2"></param>
//        /// <returns></returns>
//        public static double GetSlope(int x0, int y0, int x1, int y1, int x2, int y2)
//        {
//            double slope1 = Slope(new Point(x0, y0), new Point(x1, y1));
//            double slope2 = Slope(new Point(x0, y0), new Point(x2, y2));

//            return SlopeDifference(slope1, slope2);
//        }





//        /// <summary>
//        /// Visszaadja a két pont által meghatározott meredekséget fokban. [0,360]
//        /// </summary>
//        /// <param name="p1"></param>
//        /// <param name="p2"></param>
//        /// <returns></returns>
//        public static double Slope(Point p1, Point p2)
//        {
//            double dx = p2.X - p1.X;
//            double dy = p2.Y - p1.Y;
//            return 180 + Math.Atan2(dy, dx) * 180 / Math.PI;
//        }

//        /// <summary>
//        /// Visszaadja a [0,360] fokban adott szögek különbségét fokban. [0,180]
//        /// </summary>
//        /// <param name="slope1"></param>
//        /// <param name="slope2"></param>
//        /// <returns></returns>
//        public static double SlopeDifference(double slope1, double slope2)
//        {
//            double diff = slope1 - slope2;
//            diff = Math.Abs(diff);

//            //A különségnek helyesnek kell lennie a 0,360 határ két oldalán is
//            diff = diff > 180 ? Math.Abs(360 - diff) : diff;

//            return diff;
//        }

//        public static void MirrorY<T>(this IEnumerable<T> points, int mirrorY) where T : IPoint
//        {
//            foreach (var p in points)
//            {
//                p.Y = mirrorY + (mirrorY - p.Y);
//            }
//        }

//        public static void Rectangle<T>(this IEnumerable<T> points, Rectangle source, Rectangle target) where T : IPoint
//        {
//            Scale(points, source.Left, source.Top, source.Width, source.Height, target.Top, target.Width, target.Height, target.Width);
//        }

//        public static void Scale<T>(this IEnumerable<T> points, int left1, int top1, int width1, int height1, int left2, int top2, int width2, int height2) where T : IPoint
//        {
//            double scaleX = (double)width2 / (double)width1;
//            double scaleY = (double)height2 / (double)height1;
//            foreach (IPoint p in points)
//            {
//                p.X = (int)((p.X - left1) * scaleX + left2);
//                p.Y = (int)((p.Y - top1) * scaleY + top2);
//            }
//        }


//        /// <summary>
//        /// Azokat a pontokat adja cska vissza, amelyek az adott téglalapon belülre esnek (beleírtve a téglalap határait is)
//        /// </summary>
//        /// <typeparam name="T">ponttípus</typeparam>
//        /// <param name="points">a szűrendő pontok listája</param>
//        /// <param name="left">a téglalap bal széle</param>
//        /// <param name="top">a tégelalap teteje</param>
//        /// <param name="width">a téglalap szélessége</param>
//        /// <param name="height">a téglalap magassága</param>
//        /// <returns></returns>
//        public static IEnumerable<T> ClipRectangle<T>(this IEnumerable<T> points, int left, int top, int width, int height) where T : IPoint
//        {
//            return points.Where(p => p.Y >= top && p.X >= left && p.X < left + width && p.Y < top + height);
//        }



//        internal static IEnumerable<Point> GetPerimeter(IEnumerable<Point> shapePoints)
//        {
//            // http://en.wikipedia.org/wiki/Moore_neighborhood
//            var bounds = GetBounds(shapePoints);
//            bool[,] img = new bool[bounds.Width, bounds.Height].SetValues(false); // algoritmus szerint: T
//            foreach (var sp in shapePoints)
//            {
//                img[sp.X - bounds.Left, sp.Y - bounds.Top] = true;
//            }

//            List<Point> result = new List<Point>(); // algoritmus szerint: B
//            Point s, p, b, c;
//            s = Point.Empty;
//            for (int y = 0; y < bounds.Height && s == Point.Empty; y++)
//                for (int x = 0; x < bounds.Width && s == Point.Empty; x++)
//                {
//                    if (img[x, y] == true)
//                    {
//                        s = new Point(x, y);
//                        break;
//                    }
//                }
//            if (s == Point.Empty) throw new Exception("No shape point was found.");
//            result.Add(s.Add(bounds.Left, bounds.Top));
//            b = s.Add(-1, 0);
//            p = s;
//            // Set c to be the next clockwise pixel (from b) in M(p).
//            c = p.GetNeighbours(b, 1).First();
//            while (!(c.X == s.X && c.Y == s.Y))
//            {
//                if (c.X>=0 && c.X<bounds.Width && c.Y>=0 &&c.Y<bounds.Height 
//                    && img[c.X, c.Y])
//                {
//                    result.Add(c.Add(bounds.Left, bounds.Top));
//                    b = p;
//                    p = c;
//                    // (backtrack: move the current pixel c to the pixel from which p was entered)
//                    c = p.GetNeighbours(b, 1).First();
//                }
//                else
//                {
//                    //(advance the current pixel c to the next clockwise pixel in M(p) and update backtrack)
//                    b = c;
//                    c = p.GetNeighbours(b, 1).First();
//                }
//            }
//            return result;
//        }

//        /// <summary>
//        /// Visszaadja a ponthalmaz legkisebb befoglaló téglalapját
//        /// </summary>
//        /// <param name="points"></param>
//        /// <returns></returns>
//        public static Rectangle GetBounds(this IEnumerable<Point> points)
//        {
//            int x1 = points.Min(p => p.X);
//            int x2 = points.Max(p => p.X);
//            int y1 = points.Min(p => p.Y);
//            int y2 = points.Max(p => p.Y);
//            return new Rectangle(x1, y1, x2 - x1 + 1, y2 - y1 + 1);

//        }

//        public static IEnumerable<Point> GetEndPoints(byte[,] rawImage)
//        {
//            byte[,] neighbourCount = Matrix.Neighbours(rawImage, (byte)0);

//            for (int x = 0; x < rawImage.GetLength(0); x++)
//            {
//                for (int y = 0; y < rawImage.GetLength(1); y++)
//                {
//                    Point target = new Point(x, y);

//                    if (rawImage[x, y] > 0 && neighbourCount[x, y] == 1)
//                        yield return target;
//                }
//            }
//        }
//    }
//}
