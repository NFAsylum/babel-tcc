// Showcase.cs - demonstracao abrangente do Babel TCC.
// Objetivo: exercitar todas as 89 keywords C# suportadas e anotacoes "tradu"
// multi-idioma (// tradu[lang]:...|[lang]:...) nos idiomas suportados
// (exceto pt-br-ascii, que e variacao do pt-br). O arquivo permanece C# valido;
// a traducao e apenas visual.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Showcase // tradu[pt-br]:Vitrine|[es-es]:Vitrina|[fr-fr]:Vitrine|[de-de]:Schaufenster|[it-it]:Vetrina|[en-us]:Showcase|[ja-jp-romaji]:Tenji|[zh-cn]:展示|[ar-sa]:عرض
{
    // delegate + event
    public delegate void ShapeChanged(string name); // tradu[pt-br]:FormaAlterada|[es-es]:FormaCambiada|[fr-fr]:FormeModifiee|[de-de]:FormGeaendert|[it-it]:FormaCambiata|[en-us]:ShapeChanged|[ja-jp-romaji]:KatachiHenkou|[zh-cn]:形状已更改|[ar-sa]:تغير_الشكل

    public enum ShapeKind // tradu[pt-br]:TipoForma|[es-es]:TipoForma|[fr-fr]:TypeForme|[de-de]:FormArt|[it-it]:TipoForma|[en-us]:ShapeKind|[ja-jp-romaji]:KatachiShurui|[zh-cn]:形状类型|[ar-sa]:نوع_الشكل
    {
        Circle,
        Square,
        Triangle
    }

    public interface IShape // tradu[pt-br]:IForma|[es-es]:IForma|[fr-fr]:IForme|[de-de]:IForm|[it-it]:IForma|[en-us]:IShape|[ja-jp-romaji]:IKatachi|[zh-cn]:形状接口|[ar-sa]:واجهة_الشكل
    {
        double Area();
    }

    // abstract / virtual / protected / readonly / const / field
    public abstract class Shape : IShape // tradu[pt-br]:Forma|[es-es]:Forma|[fr-fr]:Forme|[de-de]:Form|[it-it]:Forma|[en-us]:Shape|[ja-jp-romaji]:Katachi|[zh-cn]:形状|[ar-sa]:شكل
    {
        public const double Pi = 3.14159;
        protected readonly ShapeKind kind; // tradu[pt-br]:tipo|[es-es]:tipo|[fr-fr]:type|[de-de]:art|[it-it]:tipo|[en-us]:kind|[ja-jp-romaji]:shurui|[zh-cn]:类型|[ar-sa]:نوع

        protected Shape(ShapeKind kind)
        {
            this.kind = kind;
        }

        public abstract double Area(); // tradu[pt-br]:Area|[es-es]:Area|[fr-fr]:Aire|[de-de]:Flaeche|[it-it]:Area|[en-us]:Area|[ja-jp-romaji]:Menseki|[zh-cn]:面积|[ar-sa]:مساحة

        public virtual string Describe() // tradu[pt-br]:Descrever|[es-es]:Describir|[fr-fr]:Decrire|[de-de]:Beschreiben|[it-it]:Descrivere|[en-us]:Describe|[ja-jp-romaji]:Setsumei|[zh-cn]:描述|[ar-sa]:وصف
        {
            return $"{kind} with area {Area()}"; // tradu[pt-br]:"{kind} com area {Area()}"|[es-es]:"{kind} con area {Area()}"|[fr-fr]:"{kind} avec aire {Area()}"|[de-de]:"{kind} mit Flaeche {Area()}"|[it-it]:"{kind} con area {Area()}"
        }
    }

    // sealed + override + base
    public sealed class Circle : Shape // tradu[pt-br]:Circulo|[es-es]:Circulo|[fr-fr]:Cercle|[de-de]:Kreis|[it-it]:Cerchio|[en-us]:Circle|[ja-jp-romaji]:En|[zh-cn]:圆形|[ar-sa]:دائرة
    {
        private readonly double radius; // tradu[pt-br]:raio|[es-es]:radio|[fr-fr]:rayon|[de-de]:radius|[it-it]:raggio|[en-us]:radius|[ja-jp-romaji]:hankei|[zh-cn]:半径|[ar-sa]:شعاع

        public Circle(double radius) : base(ShapeKind.Circle)
        {
            this.radius = radius;
        }

        public override double Area() // tradu[pt-br]:Area|[es-es]:Area|[fr-fr]:Aire|[de-de]:Flaeche|[it-it]:Area|[en-us]:Area|[ja-jp-romaji]:Menseki|[zh-cn]:面积|[ar-sa]:مساحة
        {
            return Pi * radius * radius;
        }
    }

    // struct + operator + implicit + explicit + readonly struct
    public readonly struct Vector // tradu[pt-br]:Vetor|[es-es]:Vector|[fr-fr]:Vecteur|[de-de]:Vektor|[it-it]:Vettore|[en-us]:Vector|[ja-jp-romaji]:Bekutoru|[zh-cn]:向量|[ar-sa]:متجه
    {
        public readonly double X;
        public readonly double Y;

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y);
        }

        public static implicit operator double(Vector v) => v.X;
        public static explicit operator int(Vector v) => (int)v.X;
    }

    // record + init + required
    public record Point // tradu[pt-br]:Ponto|[es-es]:Punto|[fr-fr]:Point|[de-de]:Punkt|[it-it]:Punto|[en-us]:Point|[ja-jp-romaji]:Ten|[zh-cn]:点|[ar-sa]:نقطة
    {
        public required int X { get; init; }
        public required int Y { get; init; }
    }

    // partial class (continua a definicao da classe Engine)
    public partial class Engine // tradu[pt-br]:Motor|[es-es]:Motor|[fr-fr]:Moteur|[de-de]:Motor|[it-it]:Motore|[en-us]:Engine|[ja-jp-romaji]:Enjin|[zh-cn]:引擎|[ar-sa]:محرك
    {
        private static volatile bool running;
        private readonly object gate = new object();
        public event ShapeChanged OnChange;

        public void Notify(string name) // tradu[pt-br]:Notificar|[es-es]:Notificar|[fr-fr]:Notifier|[de-de]:Benachrichtigen|[it-it]:Notificare|[en-us]:Notify|[ja-jp-romaji]:Tsuuchi|[zh-cn]:通知|[ar-sa]:إشعار
        {
            OnChange?.Invoke(name);
        }

        // tipos numericos e var/dynamic
        public void Numbers()
        {
            bool flag = true;
            byte b = 1;
            sbyte sb = -1;
            short sh = 2;
            ushort ush = 3;
            int i = 4;
            uint ui = 5;
            long l = 6L;
            ulong ul = 7UL;
            float f = 1.0f;
            double d = 2.0;
            decimal m = 3.0m;
            char c = 'x';
            string s = "text";
            object o = s;
            dynamic dyn = 10;
            var inferred = 20;

            if (flag == false || o == null)
            {
                return;
            }

            Console.WriteLine($"{b}{sb}{sh}{ush}{i}{ui}{l}{ul}{f}{d}{m}{c}{inferred}{dyn}");
        }

        // controle de fluxo: for/foreach/in/while/do/switch/case/default/break/continue/goto/return
        public int Control(IEnumerable<int> items)
        {
            int total = 0;
            for (int i = 0; i < 3; i++)
            {
                if (i == 1)
                {
                    continue;
                }
                else
                {
                    total += i;
                }
            }

            foreach (var item in items)
            {
                total += item;
            }

            int n = 0;
            while (n < 2)
            {
                n++;
            }

            do
            {
                n--;
            }
            while (n > 0);

            switch (total)
            {
                case 0:
                    goto done;
                default:
                    break;
            }

        done:
            return total;
        }

        // excecoes + lock + checked/unchecked + is/as/typeof/sizeof/nameof + default
        public string Operators(object value)
        {
            lock (gate)
            {
                running = true;
            }

            try
            {
                int big = unchecked(int.MaxValue + 1);
                int safe = checked(1 + 1);

                if (value is string str)
                {
                    return str;
                }

                var maybe = value as string;
                Type t = typeof(Engine);
                int size = sizeof(int);
                string name = nameof(Operators);
                int fallback = default;

                return $"{maybe}{t.Name}{size}{name}{big}{safe}{fallback}";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("failed", ex);
            }
            finally
            {
                running = false;
            }
        }

        // params / ref / out / in
        public int Params(params int[] values) // tradu[pt-br]:Parametros|[es-es]:Parametros|[fr-fr]:Parametres|[de-de]:Parameter|[it-it]:Parametri|[en-us]:Params|[ja-jp-romaji]:Hikisuu|[zh-cn]:参数|[ar-sa]:معاملات
        {
            int sum = 0;
            foreach (var v in values)
            {
                sum += v;
            }
            return sum;
        }

        public void RefOutIn(ref int a, out int b, in int c)
        {
            b = a + c;
            a = b;
        }

        // generico + where
        public T First<T>(IList<T> list) where T : class
        {
            return list[0];
        }

        // yield
        public IEnumerable<int> Counter()
        {
            yield return 1;
            yield return 2;
        }

        // async / await
        public async Task<int> LoadAsync()
        {
            await Task.Delay(1);
            return 42;
        }

        // unsafe / fixed / stackalloc
        public unsafe int Unsafe()
        {
            int* buffer = stackalloc int[3];
            int[] data = { 7, 8, 9 };
            fixed (int* p = data)
            {
                buffer[0] = *p;
            }
            return buffer[0];
        }

        // extern (P/Invoke)
        [DllImport("libc")]
        private static extern int getpid();
    }

    // segunda metade da classe partial + global + new (modifier)
    public partial class Engine
    {
        internal static int PidViaGlobal()
        {
            return global::System.Environment.ProcessId;
        }

        public bool IsRunning() => running;

        public class Base { public virtual void Run() { } }
        public class Derived : Base { public new void Run() { } }
    }
}
