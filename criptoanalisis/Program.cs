using System;
using System.Collections.Generic;
using System.Linq;
using WeCantSpell.Hunspell;

class CriptoanalizadorMonoalfabetico
{
    static string? FrecuenciaString { get; set; }

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

        //Console.WriteLine(texto);
        Imprimir(texto);

        string[] palabras = texto.Split(new char[] { ' ', ',', '.', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
        bool check = palabras.All(palabra => diccionario.Check(palabra.ToLower())); // Convertir a minúsculas antes de verificar
        return check;
    }

    static Dictionary<char, char> OptimizarMapeo(string textoCifrado, Dictionary<char, int> frecuencias, WordList diccionario)
    {
        var mapeo = GenerarMapeoAutomatico(frecuencias);
        var letrasBloqueadas = new HashSet<char>(); // Conjunto para letras bloqueadas

        // Función recursiva para explorar combinaciones
        bool ExplorarCombinaciones(Dictionary<char, char> mapeoActual, int indiceLetra)
        {
            if (indiceLetra == mapeoActual.Count)
            {
                string textoDescifrado = SustituirLetras(textoCifrado, mapeoActual);
                return TextoEsValido(textoDescifrado, diccionario); // Todas las palabras válidas
            }

            char letraCifrada = mapeoActual.Keys.ElementAt(indiceLetra);
            if (letrasBloqueadas.Contains(letraCifrada))
                return ExplorarCombinaciones(mapeoActual, indiceLetra + 1); // Saltar letra bloqueada

            foreach (char nuevaLetra in "abcdefghijklmnopqrstuvwxyzñ")
            {
                if (nuevaLetra != mapeoActual[letraCifrada] && !mapeoActual.Values.Contains(nuevaLetra))
                {
                    var nuevoMapeo = new Dictionary<char, char>(mapeoActual);
                    nuevoMapeo[letraCifrada] = nuevaLetra;

                    string textoDescifrado = SustituirLetras(textoCifrado, nuevoMapeo);
                    if (TextoEsValido(textoDescifrado, diccionario))
                    {
                        // Bloquear letras de palabras válidas
                        foreach (var palabra in textoDescifrado.Split(new char[] { ' ', ',', '.', ';', ':' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (diccionario.Check(palabra.ToLower()))
                                foreach (var letra in palabra)
                                    letrasBloqueadas.Add(letra);
                        }

                        mapeo = nuevoMapeo; // Actualizar el mejor mapeo
                        return true; // Éxito, no es necesario seguir explorando
                    }
                    else if (ExplorarCombinaciones(nuevoMapeo, indiceLetra + 1))
                        return true; // Éxito en una rama más profunda
                }
            }

            return false; // No se encontró solución en esta rama
        }

        ExplorarCombinaciones(mapeo, 0); // Iniciar la exploración recursiva
        return mapeo;
    }

    static void Imprimir(string texto)
    {
        Thread.Sleep(1);
        Console.SetCursorPosition(0, Console.CursorTop - 1);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.WriteLine(texto);
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
        //string textoCifrado = "TATIG NK KTIUTZXGJU ATG VKXYUTG ZGT OMTUXGTZK WAK TU YK VAKJG GVXKTJKX TGJG JK KRRG";
        string textoCifrado = "TATIG";
        var frecuencias = AnalizarFrecuencias(textoCifrado.ToLower());

        // Variable global frecuencias
        FrecuenciaString = "Frecuencias de letras: \n";
        foreach (var par in frecuencias.OrderByDescending(x => x.Value))
        {
            FrecuenciaString += $"{par.Key}: {par.Value} \n";
        }
        FrecuenciaString += "\n";
        Console.WriteLine(FrecuenciaString);

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
