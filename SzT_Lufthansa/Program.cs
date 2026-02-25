using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SzT_Lufthansa {
    internal class Program {
        struct Csomag {
            public string id;
            public double suly;
            public double terfogat;
        }

        struct csaladiCsomagok {
            public string id;
            public double osszSuly;
            public double osszTerfogat;
            public List<Csomag> csomagok;
        }

        struct Kontener {
            public int id;
            public int erokar;
            public double suly;
            public double terfogat;
            public List<csaladiCsomagok> betoltott;
        }

        static Dictionary<string, csaladiCsomagok> beolvas(string filename) {
            Dictionary<string, csaladiCsomagok> l = new Dictionary<string, csaladiCsomagok>();
            StreamReader sr = new StreamReader("../../" + filename);

            while (!sr.EndOfStream) {
                string[] adat = sr.ReadLine().Split(';');

                Csomag cs;
                cs.id = adat[0];
                cs.suly = Convert.ToDouble(adat[2].Replace(".",","));
                cs.terfogat = Convert.ToDouble(adat[3].Replace(".", ","));

                if (l.ContainsKey(adat[1])) {
                    csaladiCsomagok cscs = l[adat[1]];

                    cscs.osszSuly += cs.suly;
                    cscs.osszTerfogat += cs.terfogat;
                    cscs.csomagok.Add(cs);

                    l[adat[1]] = cscs;
                } else {
                    csaladiCsomagok cscs;
                    cscs.id = adat[1];
                    cscs.osszSuly = cs.suly;
                    cscs.osszTerfogat = cs.terfogat;
                    cscs.csomagok = new List<Csomag>();
                    cscs.csomagok.Add(cs);

                    l.Add(adat[1], cscs);
                }

            }

            sr.Close();
            return l;
        }

        static Kontener[] inicializal() {
            Kontener[] k = new Kontener[5];

            for (int i = 0; i < k.Length; i++) {
                Kontener kont;
                kont.id = i+1;
                kont.erokar = i - 2;
                kont.suly = 0;
                kont.terfogat = 0;
                kont.betoltott = new List<csaladiCsomagok>();

                k[i] = kont;
            }

            return k;
        }

        static bool beleferE(Kontener k, csaladiCsomagok cscs) {
            double maxSuly = 1500;
            double maxTerfogat = 6;

            return (k.suly + cscs.osszSuly <= maxSuly && k.terfogat + cscs.osszTerfogat <= maxTerfogat);
        }

        static csaladiCsomagok nehez(Dictionary<string, csaladiCsomagok> lista, int k) {
            List<csaladiCsomagok> l = new List<csaladiCsomagok>();

            foreach (KeyValuePair<string, csaladiCsomagok> item in lista) {
                l.Add(item.Value);
            }

            for (int i = 0; i < l.Count-1; i++) {
                for (int j = i; j < l.Count; j++) {
                    if (l[i].osszSuly < l[j].osszSuly) {
                        csaladiCsomagok sv = l[i];
                        l[i] = l[j];
                        l[j] = sv;
                    }
                }
            }

            return l[k];
        }

        static double hataroz(Kontener[] kont) {
            double szamlalo = 0;
            double nevezo = 0;

            for (int i = 0; i < kont.Length; i++) {
                szamlalo += kont[i].suly * kont[i].erokar;
                nevezo += kont[i].suly;
            }

            double cg = szamlalo / nevezo;

            return cg;
        }

        static Kontener[] algoritmus(Dictionary<string, csaladiCsomagok> l, Kontener[] k) {
            for (int i = 0; i < l.Count; i++) {
                csaladiCsomagok cscs = nehez(l, i);

                double minCg = double.MaxValue;
                int minI = -1;

                for (int j = 0; j < k.Length; j++) {
                    if (beleferE(k[j], cscs)) {

                        k[j].suly += cscs.osszSuly;
                        double cg = Math.Abs(hataroz(k));

                        if (cg < minCg) {
                            minCg = cg;
                            minI = j;
                        }

                        k[j].suly -= cscs.osszSuly;
                    }
                }

                if (minI != -1) {
                    Kontener kk = k[minI];
                    kk.suly += cscs.osszSuly;
                    kk.terfogat += cscs.osszTerfogat;
                    kk.betoltott.Add(cscs);
                    k[minI] = kk;
                }
            }
            return k;
        }

        static void kiiras(Kontener[] k) {
            Console.WriteLine("--- LUFTHANSA JÁRAT RAKODÁSI TERV ---");

            for (int i = 0; i < k.Length; i++) {
                Console.WriteLine($"Konténer {i+1} (Erőkar: {k[i].erokar}): {Math.Round(k[i].suly,2)} kg / {Math.Round(k[i].terfogat,2)} m^3 - {k[i].betoltott.Count} család");
            }

            double cg = hataroz(k);
            Console.WriteLine($"A repülőgép VÉGSŐ súlypontja (CG): {Math.Round(cg,4)}");
            if (Math.Abs(cg) < 0.5) {
                Console.WriteLine("A gép tökéletes egyensúlyban van. Felszállás engedélyezve!");
            }
            else {
                Console.WriteLine("A gép nincs egyensúlyban. Felszállás megtiltva!");
            }
        }

        static void Main(string[] args) {
            Dictionary<string, csaladiCsomagok> lista = beolvas("csomagok.csv");
            Kontener[] kontenerek = inicializal();
            kontenerek = algoritmus(lista, kontenerek);

            kiiras(kontenerek);
        }
    }
}
