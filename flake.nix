{
  description = "Mawtrix - A Matrix TUI client";
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
    flake-utils.url = "github:numtide/flake-utils";
    flake-compat = {
      url = "github:edolstra/flake-compat";
      flake = false;
    };
  };
  outputs = { self, nixpkgs, flake-utils, flake-compat }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = nixpkgs.legacyPackages.${system};
        dotnetPackages = pkgs.dotnetCorePackages;
        dotnetSdk = dotnetPackages.sdk_8_0;
        dotnetRuntime = dotnetPackages.runtime_8_0;
      in {
        packages.default = pkgs.buildDotnetModule {
          pname = "mawtrix";
          version = "0.7";
          src = ./.;
          projectFile = "Mawtrix/Mawtrix.csproj";
          nugetDeps = ./deps.json;
          dotnet-sdk = dotnetSdk;
          dotnet-runtime = dotnetRuntime;
          buildInputs = with pkgs; [ openssl ];
          executables = [ "Mawtrix" ];
          meta = {
            description = "Matrix TUI client";
            mainProgram = "Mawtrix";
          };
        };
        apps.default = {
          type = "app";
          program = "${self.packages.${system}.default}/bin/Mawtrix";
        };
        devShells.default = pkgs.mkShell {
          packages = with pkgs; [
            dotnetSdk
            dotnetRuntime
            openssl
            nuget-to-json
          ];
        };
      }
    );
}
