# Mawtrix: The [MATRIX] C# Client.
Mawtrix is a simple [matrix] TUI i made on C# because i was very bored.
I wanted to practice and check out how hard it would be to make a Matrix client, turns out, it's not that hard after all.
#### *Maw*trix
### Mawtrix currently supports:
- Logging in through any homeserver
- Not having to put your info in again (fast relogin)
- DMing people
- Joining public rooms by alias ``#alias:homeserver.example``
- Images (only ones others send) (only on terminals like foot that support the image protocol)

## Building Mawtrix
> [!NOTE]
> You do not have to build it to use it! All the latest stuff is on [GITHUB ACTIONS](https://github.com/Entarno54/Mawtrix/actions)
1. Install .Net
2. Install Git
```
git clone https://github.com/Entarno54/Mawtrix.git
cd Mawtrix
dotnet restore
dotnet build --configuration Release --no-restore
```
3. See your built stuff in ``Mawtrix/Mawtrix/bin/Release/net8.0``!

## Nix stuffz for forkz
### Testing
```
nix build        # build the package
nix run          # run Mawtrix
nix develop      # enter dev shell
```
> [!NOTE]
>  deps.json should be regenerated via nuget-to-json if you change NuGet dependencies on your fork.
> 
>  shell.nix is a stub for nix-shell compat; the flake devShell is canonical.
