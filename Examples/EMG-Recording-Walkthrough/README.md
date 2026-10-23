# EMG recording walkthrough

A Jupyter notebook that loads an ONrecorder CSV recording (EMG1) and walks through:

- the CSV layout: header, the raw row, and what the second row actually contains
- checking the true sampling rate (6400 Hz) against the header
- finding pauses and dropped BLE packets from the host timestamps
- removing 60 Hz mains interference
- detecting, blanking and characterising stimulation artifacts, and stimulus-triggered averaging

## Running it

```bash
pip install -r requirements.txt
jupyter notebook EMG_Recording_Walkthrough.ipynb
```

The notebook is saved with its outputs, so it can also be read directly on GitHub. To analyse your own recording, change `CSV_PATH` in the first code cell.

## Data

`data/Rec005.csv` is an example EMG1 recording saved by ONrecorder. It has a baseline segment and two segments recorded with stimulation on, separated by pauses in saving.
