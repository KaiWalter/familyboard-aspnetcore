{
  description = "VS Code with .NET and GitHub Copilot support";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs {
          inherit system;
          config.allowUnfree = true;
        };

        dotnetRoot = "${pkgs.dotnet-sdk_8}/share/dotnet";

        extensions = with pkgs.vscode-extensions; [
          ms-dotnettools.csharp
          ms-dotnettools.vscode-dotnet-runtime
          github.copilot
        ];

        vscode-with-extensions = pkgs.vscode-with-extensions.override {
          vscodeExtensions = extensions;
        };

      in
      {
        devShells.default = pkgs.mkShell {
          packages = [
            vscode-with-extensions
            pkgs.dotnet-sdk_9
            pkgs.dotnet-sdk_8
          ];

          shellHook = ''
            # Export DOTNET_ROOT environment variable for runtime resolution
            export DOTNET_ROOT="${dotnetRoot}"

            echo "Launching VS Code in current directory..."
            ${vscode-with-extensions}/bin/code .
            exit
          '';
        };
      }
    );
}
