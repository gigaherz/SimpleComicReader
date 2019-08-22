# SimpleComicReader

A minimalistic comic book reader designed to work from folders.

Windows-only unless someone ports WPF+WIC to another platform.

## Supported file formats

Supports the most common archive formats:
* CBR: Images in a RAR archive
* CBZ: Images in a ZIP archive
* CBT: Images in a TAR archive
* CB7: Images in a 7Z archive

Image file formats:
* JPEG/PNG/GIF/...: navitely supported by WIC (Windows Imaging Component)
* WEBP: requires a codec to be installed, such as [this one from Google](https://storage.googleapis.com/downloads.webmproject.org/releases/webp/WebpCodecSetup.exe)
* Other formats: only if WIC Codecs are available.
