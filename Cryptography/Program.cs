using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Kurukuru;

namespace Cryptography
{
    class Program
    {
        private static Random randomizer = new Random();
        private const int keysLength = 16;

        static void Main(string[] args)
        {
            var app = new CommandLineApplication(true);
            app.Name = "crypter";
            app.HelpOption("-?|-h|--help");

            app.Command("keygen", GenerateKeyCommand);
            app.Command("encrypt", EncryptFile);
            app.Command("decrypt", DecryptFile);

            app.Execute(args);
        }

        private static void GenerateKeyCommand(CommandLineApplication app)
        {
            app.Description = "Generates key pair with given length";
            app.HelpOption("-?|-h|--help");
            var lengthOpt = app.Option("-l|--length", "Key length in bytes", CommandOptionType.SingleValue);
            var fileOpt = app.Option("-o|--output", "Name of output files", CommandOptionType.SingleValue);
            app.OnExecute(() =>
            {
                Spinner.Start("Generating RSA keys...", (s) =>
                {
                    var generator = new KeyGenerator(lengthOpt.HasValue() ? int.Parse(lengthOpt.Value()) : 4);
                    var keys = generator.GenerateKeys();
                    s.Text = "Writing keys to files...";
                    var serializer = new JsonSerializer();
                    using (var fs = new FileStream(fileOpt.HasValue() ? $"{fileOpt.Value()}.pub" : "key.pub", FileMode.Create))
                    using (var bsonWriter = new BsonWriter(fs))
                    {
                        serializer.Serialize(bsonWriter, keys.OpenKey);
                    }
                    using (var fs = new FileStream(fileOpt.HasValue() ? $"{fileOpt.Value()}.private" : "key.private", FileMode.Create))
                    using (var bsonWriter = new BsonWriter(fs))
                    {
                        serializer.Serialize(bsonWriter, keys.ClosedKey);
                    }
                    s.Succeed("Done!");
                });
                return 0;
            });
        }

        private static void EncryptFile(CommandLineApplication app)
        {
            app.Description = "Encrypts file";
            app.HelpOption("-?|-h|--help");
            var keyArg = app.Argument("[key]", "A public key file");
            var fileArg = app.Argument("[inputFile]", "File to encrypt");
            var outOpt = app.Option("-o", "Output file", CommandOptionType.SingleValue);
            app.OnExecute(() =>
            {
                Spinner.Start("Encrypting file...", (s) =>
                {
                    OpenKey key;
                    using (var fs = new FileStream(keyArg.Value, FileMode.Open))
                    using (var reader = new BsonReader(fs))
                    {
                        var serializer = new JsonSerializer();
                        key = serializer.Deserialize<OpenKey>(reader);
                    }
                    var fileContent = File.ReadAllBytes(fileArg.Value);
                    using (var fs = new FileStream(outOpt.HasValue() ? outOpt.Value() : $"{fileArg.Value}.encrypted", FileMode.Create))
                    {
                        var encryptedContent = key.Encode(fileContent);
                        fs.Write(encryptedContent, 0, encryptedContent.Length);
                    }
                    s.Succeed("Done!");
                });
                return 0;
            });
        }

        private static void DecryptFile(CommandLineApplication app)
        {
            app.Description = "Decrypts file";
            app.HelpOption("-?|-h|--help");
            var keyArg = app.Argument("[key]", "A private key file");
            var fileArg = app.Argument("[inputFile]", "File to decrypt");
            var outOpt = app.Option("-o", "Output file", CommandOptionType.SingleValue);
            app.OnExecute(() =>
            {
                Spinner.Start("Encrypting file...", (s) =>
                {
                    ClosedKey key;
                    using (var fs = new FileStream(keyArg.Value, FileMode.Open))
                    using (var reader = new BsonReader(fs))
                    {
                        var serializer = new JsonSerializer();
                        key = serializer.Deserialize<ClosedKey>(reader);
                    }
                    var fileContent = File.ReadAllBytes(fileArg.Value);
                    using (var fs = new FileStream(outOpt.HasValue() ? outOpt.Value() : $"{fileArg.Value}.decrypted", FileMode.Create))
                    {
                        var encryptedContent = key.Decode(fileContent);
                        fs.Write(encryptedContent, 0, encryptedContent.Length);
                    }
                    s.Succeed("Done!");
                });
                return 0;
            });
        }
    }
}
