using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCompletionSample.CompletionSource
{
    [Export]
    internal class ElementCatalog
    {
        // Data from https://en.wikipedia.org/wiki/List_of_chemical_elements
        public List<Element> Elements { get; } = new List<Element>
        {
            new Element(1,"H","Hydrogen",1.008, Element.Categories.NonMetal),
            new Element(2,"He","Helium",4.002602, Element.Categories.NonMetal),
            new Element(3,"Li","Lithium",6.94, Element.Categories.Metal),
            new Element(4,"Be","Beryllium",9.0121831, Element.Categories.Metal),
            new Element(5,"B","Boron",10.81, Element.Categories.Metalloid),
            new Element(6,"C","Carbon",12.011, Element.Categories.NonMetal),
            new Element(7,"N","Nitrogen",14.007, Element.Categories.NonMetal),
            new Element(8,"O","Oxygen",15.999, Element.Categories.NonMetal),
            new Element(9,"F","Fluorine",18.998403163, Element.Categories.NonMetal),
            new Element(10,"Ne","Neon",20.1797, Element.Categories.NonMetal),
            new Element(11,"Na","Sodium",22.98976928, Element.Categories.Metal),
            new Element(12,"Mg","Magnesium",24.305, Element.Categories.Metal),
            new Element(13,"Al","Aluminium",26.9815385, Element.Categories.Metal),
            new Element(14,"Si","Silicon",28.085, Element.Categories.Metalloid),
            new Element(15,"P","Phosphorus",30.973761998, Element.Categories.NonMetal),
            new Element(16,"S","Sulfur",32.06, Element.Categories.NonMetal),
            new Element(17,"Cl","Chlorine",35.45, Element.Categories.NonMetal),
            new Element(18,"Ar","Argon",39.948, Element.Categories.NonMetal),
            new Element(19,"K","Potassium",39.0983, Element.Categories.Metal),
            new Element(20,"Ca","Calcium",40.078, Element.Categories.Metal),
            new Element(21,"Sc","Scandium",44.955908, Element.Categories.Metal),
            new Element(22,"Ti","Titanium",47.867, Element.Categories.Metal),
            new Element(23,"V","Vanadium",50.9415, Element.Categories.Metal),
            new Element(24,"Cr","Chromium",51.9961, Element.Categories.Metal),
            new Element(25,"Mn","Manganese",54.938044, Element.Categories.Metal),
            new Element(26,"Fe","Iron",55.845, Element.Categories.Metal),
            new Element(27,"Co","Cobalt",58.933194, Element.Categories.Metal),
            new Element(28,"Ni","Nickel",58.6934, Element.Categories.Metal),
            new Element(29,"Cu","Copper",63.546, Element.Categories.Metal),
            new Element(30,"Zn","Zinc",65.38, Element.Categories.Metal),
            new Element(31,"Ga","Gallium",69.723, Element.Categories.Metal),
            new Element(32,"Ge","Germanium",72.630, Element.Categories.Metalloid),
            new Element(33,"As","Arsenic",74.921595, Element.Categories.Metalloid),
            new Element(34,"Se","Selenium",78.971, Element.Categories.NonMetal),
            new Element(35,"Br","Bromine",79.904, Element.Categories.NonMetal),
            new Element(36,"Kr","Krypton",83.798, Element.Categories.NonMetal),
            new Element(37,"Rb","Rubidium",85.4678, Element.Categories.Metal),
            new Element(38,"Sr","Strontium",87.62, Element.Categories.Metal),
            new Element(39,"Y","Yttrium",88.90584, Element.Categories.Metal),
            new Element(40,"Zr","Zirconium",91.224, Element.Categories.Metal),
            new Element(41,"Nb","Niobium",92.90637, Element.Categories.Metal),
            new Element(42,"Mo","Molybdenum",95.95, Element.Categories.Metal),
            new Element(43,"Tc","Technetium",98, Element.Categories.Metal),
            new Element(44,"Ru","Ruthenium",101.07, Element.Categories.Metal),
            new Element(45,"Rh","Rhodium",102.90550, Element.Categories.Metal),
            new Element(46,"Pd","Palladium",106.42, Element.Categories.Metal),
            new Element(47,"Ag","Silver",107.8682, Element.Categories.Metal),
            new Element(48,"Cd","Cadmium",112.414, Element.Categories.Metal),
            new Element(49,"In","Indium",114.818, Element.Categories.Metal),
            new Element(50,"Sn","Tin",118.710, Element.Categories.Metal),
            new Element(51,"Sb","Antimony",121.760, Element.Categories.Metalloid),
            new Element(52,"Te","Tellurium",127.60, Element.Categories.Metalloid),
            new Element(53,"I","Iodine",126.90447, Element.Categories.NonMetal),
            new Element(54,"Xe","Xenon",131.293, Element.Categories.NonMetal),
            new Element(55,"Cs","Caesium",132.90545196, Element.Categories.Metal),
            new Element(56,"Ba","Barium",137.327, Element.Categories.Metal),
            new Element(57,"La","Lanthanum",138.90547, Element.Categories.Metal),
            new Element(58,"Ce","Cerium",140.116, Element.Categories.Metal),
            new Element(59,"Pr","Praseodymium",140.90766, Element.Categories.Metal),
            new Element(60,"Nd","Neodymium",144.242, Element.Categories.Metal),
            new Element(61,"Pm","Promethium",145, Element.Categories.Metal),
            new Element(62,"Sm","Samarium",150.36, Element.Categories.Metal),
            new Element(63,"Eu","Europium",151.964, Element.Categories.Metal),
            new Element(64,"Gd","Gadolinium",157.25, Element.Categories.Metal),
            new Element(65,"Tb","Terbium",158.92535, Element.Categories.Metal),
            new Element(66,"Dy","Dysprosium",162.500, Element.Categories.Metal),
            new Element(67,"Ho","Holmium",164.93033, Element.Categories.Metal),
            new Element(68,"Er","Erbium",167.259, Element.Categories.Metal),
            new Element(69,"Tm","Thulium",168.93422, Element.Categories.Metal),
            new Element(70,"Yb","Ytterbium",173.045, Element.Categories.Metal),
            new Element(71,"Lu","Lutetium",174.9668, Element.Categories.Metal),
            new Element(72,"Hf","Hafnium",178.49, Element.Categories.Metal),
            new Element(73,"Ta","Tantalum",180.94788, Element.Categories.Metal),
            new Element(74,"W","Tungsten",183.84, Element.Categories.Metal),
            new Element(75,"Re","Rhenium",186.207, Element.Categories.Metal),
            new Element(76,"Os","Osmium",190.23, Element.Categories.Metal),
            new Element(77,"Ir","Iridium",192.217, Element.Categories.Metal),
            new Element(78,"Pt","Platinum",195.084, Element.Categories.Metal),
            new Element(79,"Au","Gold",196.966569, Element.Categories.Metal),
            new Element(80,"Hg","Mercury",200.592, Element.Categories.Metal),
            new Element(81,"Tl","Thallium",204.38, Element.Categories.Metal),
            new Element(82,"Pb","Lead",207.2, Element.Categories.Metal),
            new Element(83,"Bi","Bismuth",208.98040, Element.Categories.Metal),
            new Element(84,"Po","Polonium",209, Element.Categories.Metal),
            new Element(85,"At","Astatine",210, Element.Categories.Metalloid),
            new Element(86,"Rn","Radon",222, Element.Categories.NonMetal),
            new Element(87,"Fr","Francium",223, Element.Categories.Metal),
            new Element(88,"Ra","Radium",226, Element.Categories.Metal),
            new Element(89,"Ac","Actinium",227, Element.Categories.Metal),
            new Element(90,"Th","Thorium",232.0377, Element.Categories.Metal),
            new Element(91,"Pa","Protactinium",231.03588, Element.Categories.Metal),
            new Element(92,"U","Uranium",238.02891, Element.Categories.Metal),
            new Element(93,"Np","Neptunium",237, Element.Categories.Metal),
            new Element(94,"Pu","Plutonium",244, Element.Categories.Metal),
            new Element(95,"Am","Americium",243, Element.Categories.Metal),
            new Element(96,"Cm","Curium",247, Element.Categories.Metal),
            new Element(97,"Bk","Berkelium",247, Element.Categories.Metal),
            new Element(98,"Cf","Californium",251, Element.Categories.Metal),
            new Element(99,"Es","Einsteinium",252, Element.Categories.Metal),
            new Element(100,"Fm","Fermium",257, Element.Categories.Metal),
            new Element(101,"Md","Mendelevium",258, Element.Categories.Metal),
            new Element(102,"No","Nobelium",259, Element.Categories.Metal),
            new Element(103,"Lr","Lawrencium",266, Element.Categories.Metal),
            new Element(104,"Rf","Rutherfordium",267, Element.Categories.Metal),
            new Element(105,"Db","Dubnium",268, Element.Categories.Metal),
            new Element(106,"Sg","Seaborgium",269, Element.Categories.Metal),
            new Element(107,"Bh","Bohrium",270, Element.Categories.Metal),
            new Element(108,"Hs","Hassium",277, Element.Categories.Metal),
            new Element(109,"Mt","Meitnerium",278, Element.Categories.Unknown),
            new Element(110,"Ds","Darmstadtium",281, Element.Categories.Unknown),
            new Element(111,"Rg","Roentgenium",282, Element.Categories.Unknown),
            new Element(112,"Cn","Copernicium",285, Element.Categories.Unknown),
            new Element(113,"Nh","Nihonium",286, Element.Categories.Unknown),
            new Element(114,"Fl","Flerovium",289, Element.Categories.Unknown),
            new Element(115,"Mc","Moscovium",290, Element.Categories.Unknown),
            new Element(116,"Lv","Livermorium",293, Element.Categories.Unknown),
            new Element(117,"Ts","Tennessine",294, Element.Categories.Unknown),
            new Element(118,"Og","Oganesson",294, Element.Categories.Unknown),
        };

        public class Element
        {
            public int AtomicNumber { get; }
            public string Symbol { get; }
            public string Name { get; }
            public double AtomicWeight { get; }
            public Categories Category { get; }

            public enum Categories
            {
                NonMetal,
                Metal,
                Metalloid,
                Unknown
            }

            internal Element(int atomicNumber, string symbol, string name, double atomicWeight, Categories category)
            {
                AtomicNumber = atomicNumber;
                Symbol = symbol;
                Name = name;
                AtomicNumber = atomicNumber;
                Category = category;
            }
        }
    }
}
