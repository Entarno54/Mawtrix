{
  description = "Development environment for Mawtrix";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = nixpkgs.legacyPackages.${system};
      in {
        defaultPackage = pkgs.mkShell {
          packages = with pkgs; [
            dotnet-sdk_8
            dotnet-runtime_8
            openssl
          ];
        };
      }
    );
}
