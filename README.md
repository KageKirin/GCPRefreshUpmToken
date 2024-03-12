# GCP Refresh UPM Token

A simple C# tool to refresh Unity's `.upmconfig.toml` with the current Google Cloud Platform token.
This tool relies on the Google Cloud SDK being installed, and notably the `gcloud` CLI being present.

## ⚡ Getting Started

Install as global tool via `dotnet tool install -g GCPRefreshUpmToken`,
or see next section.

### 🔧 Running

Example command line:
`GCPRefreshUpmToken -r https://region-location-npm.pkg.dev/organization/registry -c ~/.upmconfig.toml`

* `-r`, `--registry` precises the registry URL and is required.
* `-c`, `--config` precises the config file, i.e. `~/.upmconfig.toml`, and defaults to aforementioned file.

### 🔨 Build the Project

Out-of-the-box, `dotnet build` will rebuild from source.

### ▶ Running and Settings

**TBD**

`.gcprefresh.toml` and `.gcprefresh.json` can be used to precise the default registry and the default config file as well.

## 🤝 Collaboration

Please refer to [`COLLABORATION.md`](./COLLABORATION.md) for further details, but generally talking,
I'm open for bug fixes.
