using System;
using System.Collections.Generic;
using System.Linq;

class CriptoanalizadorMonoalfabetico
{
    static Dictionary<char, int> AnalizarFrecuencias(string textoCifrado)
    {
        var frecuencias = new Dictionary<char, int>();
        foreach (char c in textoCifrado)
        {
            if (char.IsLetter(c))
            {
                char cMinuscula = char.ToLower(c);
                frecuencias[cMinuscula] = frecuencias.GetValueOrDefault(cMinuscula, 0) + 1;
            }
        }
        return frecuencias;
    }

    static string SustituirLetras(string textoCifrado, Dictionary<char, char> mapeo)
    {
        var textoDescifrado = new System.Text.StringBuilder();
        foreach (char c in textoCifrado)
        {
            if (char.IsLetter(c))
            {
                char cMinuscula = char.ToLower(c);
                textoDescifrado.Append(mapeo.GetValueOrDefault(cMinuscula, c));
            }
            else
            {
                textoDescifrado.Append(c);
            }
        }
        return textoDescifrado.ToString();
    }

    static Dictionary<char, char> GenerarMapeoAutomatico(Dictionary<char, int> frecuencias)
    {
        // Frecuencias típicas del español (aproximadas y en orden descendente)
        string frecuenciasEspanol = "eaosrnidltcmupbgvyqhfzjñxwk";

        // Ordenar las letras cifradas por frecuencia descendente
        var letrasCifradasOrdenadas = frecuencias.Keys.OrderByDescending(c => frecuencias[c]).ToList();

        // Crear el mapeo
        var mapeo = new Dictionary<char, char>();
        for (int i = 0; i < letrasCifradasOrdenadas.Count; i++)
        {
            // Asignar la letra cifrada más frecuente a la letra española más frecuente, y así sucesivamente
            mapeo[letrasCifradasOrdenadas[i]] = frecuenciasEspanol[i];
        }

        return mapeo;
    }

    static void Main()
    {
        string textoCifrado = "TATIG NK KTIUTZXGJU ATG VKXYUTG ZGT OMTUXGTZK WAK TU YK VAKJG GVXKTJKX TGJG JK KRRG"; // Ejemplo de texto cifrado
        var frecuencias = AnalizarFrecuencias(textoCifrado);

        // Imprimir frecuencias
        Console.WriteLine("Frecuencias de letras:");
        foreach (var par in frecuencias.OrderByDescending(x => x.Value)) // Ordenar por frecuencia descendente
        {
            Console.WriteLine($"{par.Key}: {par.Value}");
        }
        Console.WriteLine();

        // Aquí debes crear el mapeo (puedes usar frecuencias del español)
        // Por ejemplo: mapeo['z'] = 'e', mapeo['p'] = 'l', ...

        //var mapeo = new Dictionary<char, char>
        //{
        //    { 'z', 'e' },
        //    { 'p', 'l' },
        //    { 'v', 'a' },
        //    { 'b', 'h' },
        //    { 's', 'o' },
        //    { 'f', 's' },
        //    { 't', 'r' },
        //    { 'd', 'u' }
        //}; 

        var mapeo = GenerarMapeoAutomatico(frecuencias);

        string textoDescifrado = SustituirLetras(textoCifrado, mapeo);
        Console.WriteLine("Texto descifrado: " + textoDescifrado);
    }
}
