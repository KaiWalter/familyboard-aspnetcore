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
          github.copilot
          ms-azuretools.vscode-docker
          ms-dotnettools.csdevkit
          ms-dotnettools.csharp
          ms-dotnettools.vscode-dotnet-runtime
          ms-vscode-remote.remote-containers
          vscodevim.vim
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
