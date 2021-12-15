using System;
using System.IO;
using System.Collections.Generic;

namespace Huffman_Tree
{
    class HuffmanTreeVertex
    {
        public HuffmanTreeVertex LeftVertex { get; set; }
        public HuffmanTreeVertex RightVertex { get; set; }
        public (int, long) Data { get; set; }

        public static void writeTree(HuffmanTreeVertex Tree)
        {
            if (Tree.Data.Item1 == -1)
                Console.Write(Tree.Data.Item2);
            else
                Console.Write("*" + Tree.Data.Item1 + ":" + Tree.Data.Item2);

            if (Tree.LeftVertex != null)
            {
                Console.Write(' ');
                writeTree(Tree.LeftVertex);
            }

            if (Tree.RightVertex != null)
            {
                Console.Write(' ');
                writeTree(Tree.RightVertex);
            }
        }

        static public byte[] codingTree = new byte[4097];
        static public int counter = 0;
        static public Dictionary<int, string> letterCodes = new Dictionary<int, string>();

        static public void createCodingTree(HuffmanTreeVertex Tree, string path)
        {
            Int64 eightBytes;
            Int64 clearFirstByte = 0x00ffffffffffffff;

            if (Tree.Data.Item1 == -1)
            {
                eightBytes = Tree.Data.Item2 << 1;
                eightBytes &= clearFirstByte;

                byte[] byteArray = BitConverter.GetBytes(eightBytes);
                foreach (byte newByte in byteArray)
                {
                    codingTree[counter] = newByte;
                    ++counter;
                }
            }

            else
            {
                eightBytes = Tree.Data.Item2 << 1;
                eightBytes |= 0x1;
                eightBytes &= clearFirstByte;

                Int64 MSB = Convert.ToInt64(Tree.Data.Item1) << 56;
                eightBytes |= MSB;
                byte[] byteArray = BitConverter.GetBytes(eightBytes);
                foreach (byte newByte in byteArray)
                {
                    codingTree[counter] = newByte;
                    ++counter;
                }
            }

            if (Tree.LeftVertex != null)
            {
                createCodingTree(Tree.LeftVertex, path + '0');
            }

            if (Tree.RightVertex != null)
            {
                createCodingTree(Tree.RightVertex, path + '1');
            }

            if (Tree.LeftVertex == null && Tree.RightVertex == null)
            {
                letterCodes.Add(Tree.Data.Item1, path);
            }

        }

    }

    class HuffmanTree
    {
        public HuffmanTreeVertex Root { get; set; }
        public int Time { get; set; }

        public HuffmanTree()
        {
            HuffmanTreeVertex root = new HuffmanTreeVertex();
            Root = root;
        }

        static public void writeFileHead(string file)
        {
            byte[] Head = { 0x7B, 0x68, 0x75, 0x7C, 0x6D, 0x7D, 0x66, 0x66 };

            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                fs.Write(Head, 0, 8);
            }
        }

        static public HuffmanTree connect(HuffmanTree firstTree, HuffmanTree secondTree)
        {
            HuffmanTree newTree = new HuffmanTree();

            newTree.Root.Data = (-1, firstTree.Root.Data.Item2 + secondTree.Root.Data.Item2);


            newTree.Root.LeftVertex = firstTree.Root;
            newTree.Root.RightVertex = secondTree.Root;

            return newTree;
        }
        static public Dictionary<int, long> charCount(string fileName)
        {
            Dictionary<int, long> occurences = new Dictionary<int, long>();

            try
            {
                using (FileStream fileReader = new FileStream(fileName, FileMode.Open))
                {
                    byte[] buffer = new byte[4096];
                    int read_bytes;

                    while ((read_bytes = fileReader.Read(buffer, 0, 4096)) != 0)
                    {
                        for (int i = 0; i < read_bytes; i++)
                        {
                            if (occurences.ContainsKey(buffer[i]))
                                occurences[buffer[i]]++;
                            else
                                occurences.Add(buffer[i], 1);
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("File Error");
            }

            return occurences;
        }

        static public HashSet<HuffmanTree> initializeOneVertexTrees(Dictionary<int, long> charCount)
        {
            HashSet<HuffmanTree> huffmanTrees = new HashSet<HuffmanTree>();

            foreach (var pair in charCount)
            {
                HuffmanTree newTree = new HuffmanTree();

                newTree.Root.Data = (pair.Key, pair.Value);
                huffmanTrees.Add(newTree);
            }

            return huffmanTrees;
        }

        static public HuffmanTree findMinTree(HashSet<HuffmanTree> huffmanTrees)
        {
            HuffmanTree minTree = new HuffmanTree();
            minTree.Root.Data = (-1, -1);
            foreach (HuffmanTree Tree in huffmanTrees)
            {
                if (minTree.Root.Data.Item2 == -1)
                    minTree = Tree;
                else if (Tree.Root.Data.Item2 < minTree.Root.Data.Item2)
                    minTree = Tree;
                else if (Tree.Root.Data.Item2 == minTree.Root.Data.Item2)
                {
                    if (Tree.Root.Data.Item1 != -1 && minTree.Root.Data.Item1 == -1)
                        minTree = Tree;
                    else if (Tree.Root.Data.Item1 < minTree.Root.Data.Item1 && Tree.Root.Data.Item1 != -1)
                        minTree = Tree;
                    else if (Tree.Root.Data.Item1 == -1 && minTree.Root.Data.Item1 == -1)
                        if (Tree.Time < minTree.Time)
                            minTree = Tree;

                }
            }
            return minTree;
        }

        static public HashSet<HuffmanTree> createFinalTree(HashSet<HuffmanTree> huffmanTrees)
        {

            int Time = 0;

            while (huffmanTrees.Count != 1)
            {

                HuffmanTree minTree = new HuffmanTree();
                HuffmanTree secondMinTree = new HuffmanTree();

                minTree = findMinTree(huffmanTrees);
                huffmanTrees.Remove(minTree);

                secondMinTree = findMinTree(huffmanTrees);
                huffmanTrees.Remove(secondMinTree);

                HuffmanTree newTree = HuffmanTree.connect(minTree, secondMinTree);
                newTree.Time = Time++;
                huffmanTrees.Add(newTree);
            }

            return huffmanTrees;
        }
        public static byte convertToDecimal(string byteInString)
        {
            byte sum = 0;
            for (int i = 0; i < 8; i++)
            {
                sum <<= 1;
                if (byteInString[7 - i] == '1')
                {
                    sum += 1;
                }
            }
            return sum;

        }
        static public void writeCodedData(Dictionary<int, string> letterCodes, string file)
        {
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                byte[] buffer = new byte[4096];
                int read_bytes;

                string byteToWrite;
                int counter = 0;
                string byteToConvert = "";
                byte[] bufferToWrite = new byte[4096];

                using (FileStream fr = new FileStream(file + ".huff", FileMode.Open, FileAccess.Write))
                {
                    fr.Seek(0, SeekOrigin.End);
                    while ((read_bytes = fs.Read(buffer, 0, 4096)) != 0)
                    {
                        for (int i = 0; i < read_bytes; i++)
                        {

                            byteToConvert += letterCodes[buffer[i]];
                            while (byteToConvert.Length >= 8)

                            {
                                byteToWrite = byteToConvert.Substring(0, 8);

                                bufferToWrite[counter] = convertToDecimal(byteToWrite);

                                byteToConvert = byteToConvert.Substring(8);
                                counter++;
                                if (counter == 4096)
                                {
                                    fr.Write(bufferToWrite, 0, counter);
                                    counter = 0;
                                }
                            }
                        }
                    }


                    if (byteToConvert.Length != 0)
                    {

                        byteToWrite = byteToConvert.Substring(0);
                        for (int i = 0; i < 8 - byteToConvert.Length; i++)
                            byteToWrite += '0';


                        bufferToWrite[counter] = convertToDecimal(byteToWrite);
                        ++counter;
                    }

                    fr.Write(bufferToWrite, 0, counter);
                }
                }
            }
        }
    



    class Program
    {

        static void Main(string[] args)
        {
            /**/
            if (args.Length == 1)
            {
                Dictionary<int, long> occurences = HuffmanTree.charCount(args[0]);
                if (occurences.Count != 0)
                {
                    HashSet<HuffmanTree> initialForrest = HuffmanTree.initializeOneVertexTrees(occurences);
                    HashSet<HuffmanTree> finalForrest = HuffmanTree.createFinalTree(initialForrest);
                    foreach (HuffmanTree finalTree in finalForrest)
                    {
                        HuffmanTreeVertex.createCodingTree(finalTree.Root, "");
                    }
                    HuffmanTree.writeFileHead(args[0] + ".huff");
                    using (FileStream fs = new FileStream(args[0] + ".huff", FileMode.Open, FileAccess.Write))
                    {
                        fs.Seek(0, SeekOrigin.End);
                        byte zeroes = 0x0;
                        for (int i = 0; i < 8; i++)
                        {
                            HuffmanTreeVertex.codingTree[HuffmanTreeVertex.counter + i] = zeroes;
                        }

                        fs.Write(HuffmanTreeVertex.codingTree, 0, HuffmanTreeVertex.counter + 8);
                    }
                    HuffmanTree.writeCodedData(HuffmanTreeVertex.letterCodes, args[0]);
                }
            }

            else Console.WriteLine("Argument Error");

        }
    }
}