# Wav Maker

A small command-line tool that converts MP3 files to 16-bit PCM WAV files.

Born from being so bored at work that email notifications needed to become
Yoshi "MLEM" noises to keep their listener awake. Unfortunately, duty demanded a WAV file. The Wav Maker answered the call.

## Requirements

- .NET 10 SDK
- Your own MP3 files and a perfectly reasonable need to make email less boring.

## Usage

Run these commands from the project folder:

```powershell
# Convert one file. MLEM responsibly.
dotnet run -- "C:\Music\mlem.mp3"

# Convert MP3s directly inside a folder.
dotnet run -- "C:\Music"

# Choose an output folder; add -f to overwrite existing WAVs.
dotnet run -- "C:\Music" -o "C:\Music\Wavs" -f

# Show help.
dotnet run -- --help
```

You can supply multiple files or folders. Quote paths containing spaces.

## What to expect

- WAVs keep the input filename and go next to the MP3 unless you specify `-o`.
- Folder searches are **not recursive**: subfolders are left in peace.
- Existing WAVs are skipped unless you use `-f`.
- Non-MP3 file inputs are rejected. Failed inputs are listed with their reasons,  and conversion continues with the remaining files.
- With `-f`, a failed conversion can remove an existing destination WAV.  Keep a backup of any irreplaceable MLEMs.
