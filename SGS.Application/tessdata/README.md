# Tesseract language data

Download `eng.traineddata` and, if needed, `ukr.traineddata` from
[tessdata_fast](https://github.com/tesseract-ocr/tessdata_fast) into this directory.
Use the same model versions for all runs in an evaluation.

The project copies `*.traineddata` files into the `tessdata` directory of the
build and publish outputs, including applications and tests referencing this
project. No absolute path or machine-specific configuration is required.
