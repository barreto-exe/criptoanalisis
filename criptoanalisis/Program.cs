using System;
using System.Collections.Generic;
using System.Linq;
using WeCantSpell.Hunspell;

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

    static WordList CargarDiccionarioEspanol(string rutaAff, string rutaDic)
    {
        try
        {
            return WordList.CreateFromFiles(rutaDic);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar el diccionario: {ex.Message}");
            return null;
        }
    }

    static bool TextoEsValido(string texto, WordList diccionario)
    {
        if (diccionario == null) return false;

        string[] palabras = texto.Split(new char[] { ' ', ',', '.', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
        bool check = palabras.All(palabra => diccionario.Check(palabra.ToLower())); // Convertir a minúsculas antes de verificar
        return check;
    }

    static Dictionary<char, char> OptimizarMapeo(string textoCifrado, Dictionary<char, int> frecuencias, WordList diccionario)
    {
        var mapeo = GenerarMapeoAutomatico(frecuencias); // Mapeo inicial basado en frecuencias
        string mejorTexto = SustituirLetras(textoCifrado, mapeo);

        bool mejoraEncontrada;
        do
        {
            mejoraEncontrada = false;
            foreach (var par in mapeo.ToList()) // Iterar sobre una copia para poder modificar el original
            {
                foreach (char nuevaLetra in "abcdefghijklmnopqrstuvwxyzñ") // Probar todas las letras
                {
                    if (nuevaLetra != par.Value) // Evitar asignar la misma letra
                    {
                        var nuevoMapeo = new Dictionary<char, char>(mapeo);
                        nuevoMapeo[par.Key] = nuevaLetra;
                        string nuevoTexto = SustituirLetras(textoCifrado, nuevoMapeo);

                        // Verificar si el nuevo texto es válido y más largo que el mejor hasta ahora
                        if (TextoEsValido(nuevoTexto, diccionario) && nuevoTexto.Length > mejorTexto.Length)
                        {
                            mapeo = nuevoMapeo;
                            mejorTexto = nuevoTexto;
                            mejoraEncontrada = true;
                            break; // Pasar a la siguiente letra cifrada
                        }
                    }
                }
            }
        } while (mejoraEncontrada); // Repetir hasta que no se encuentren más mejoras

        return mapeo;
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
        string textoCifrado = "TATIG NK KTIUTZXGJU ATG VKXYUTG ZGT OMTUXGTZK WAK TU YK VAKJG GVXKTJKX TGJG JK KRRG";
        var frecuencias = AnalizarFrecuencias(textoCifrado);

        // Imprimir frecuencias
        Console.WriteLine("Frecuencias de letras:");
        foreach (var par in frecuencias.OrderByDescending(x => x.Value))
        {
            Console.WriteLine($"{par.Key}: {par.Value}");
        }
        Console.WriteLine();

        string rutaAff = "es_ES.aff"; // Asegúrate de tener estos archivos en la misma carpeta
        string rutaDic = "es_ES.dic";

        WordList diccionario = CargarDiccionarioEspanol(rutaAff, rutaDic);

        if (diccionario != null)
        {
            var mapeo = OptimizarMapeo(textoCifrado, frecuencias, diccionario);
            string textoDescifrado = SustituirLetras(textoCifrado, mapeo);
            Console.WriteLine("Texto descifrado: " + textoDescifrado);
        }
    }
}
