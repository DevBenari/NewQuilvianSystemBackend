# Lembar Pertanyaan untuk Pemilik Proses Gizi

| Field | Nilai |
|---|---|
| Blueprint ID | `gizi` |
| Revision | `1` |
| Status | `menunggu jawaban` |
| Ditujukan kepada | Pemilik proses gizi rumah sakit |
| Menutup | `GIZ-OQ-002`, `GIZ-OQ-004`, `GIZ-OQ-006` |

## Mengapa lembar ini ada

Desain domain modul Gizi berhenti di sini:

```text
Baca alur v1                    selesai
  |
Temukan capability existing     selesai, 11 kemampuan dipakai ulang
  |
Identifikasi kebutuhan v2       selesai, 4 kemampuan harus dibuat baru
  |
GIZ-OQ-002 belum dijawab  --+
                            +--> DESAIN DOMAIN = BERHENTI   <-- posisi sekarang
GIZ-OQ-004 belum dijawab  --+
  |
Pemilik proses gizi memutuskan
  |
Keputusan dicatat sebagai GIZ-DEC-011 dan GIZ-DEC-012
  |
Kebutuhan dinyatakan READY
  |
Desain domain baru boleh dimulai
```

Dua pertanyaan ini **tidak dapat dijawab dengan membaca kode**. Keduanya menentukan bentuk tabel
yang akan menyimpan data gizi selama bertahun-tahun ke depan. Menebaknya berarti membuat
struktur yang terlihat resmi padahal tidak pernah disahkan siapa pun, dan mengubahnya setelah
ada data sungguhan jauh lebih mahal daripada menunggu jawaban sekarang.

## Pertanyaan 1 — `GIZ-OQ-002` Isi master diagnosis gizi

**Yang sudah diputuskan.** Diagnosis gizi dipilih dari master berkode, bukan diketik bebas
(`GIZ-DEC-006`), dan master itu menumpang `MstDiagnosis` yang sudah ada dengan `DiagnosisType`
tersendiri (`GIZ-DEC-009`). Jadi wadahnya sudah siap.

**Yang belum diketahui.** Apa isinya.

### 1a. Rumah sakit memakai daftar yang mana?

| Pilihan | Keterangan |
|---|---|
| A | Standar terminologi diagnosis gizi yang berlaku nasional atau internasional. Sebutkan nama dan versinya |
| B | Daftar diagnosis gizi yang disusun sendiri oleh instalasi gizi rumah sakit |
| C | Belum ada daftar sama sekali; masih akan disusun |

Bila jawabannya **C**, modul tetap dapat dibangun, tetapi masternya akan kosong saat rilis dan
ahli gizi belum bisa memilih apa pun. Ini perlu disadari sebelum jadwal ditetapkan.

### 1b. Bentuk satu baris diagnosis

Mohon isi satu contoh nyata, bukan contoh karangan:

| Kolom | Contoh isi | Wajib? |
|---|---|---|
| Kode | | |
| Nama diagnosis | | |
| Kelompok atau domain | | |

### 1c. Apakah diagnosis gizi dikelompokkan?

Bila ya, sebutkan nama kelompoknya. Ini menentukan apakah master perlu satu kolom tambahan
untuk pengelompokan, atau cukup kode dan nama saja.

### 1d. Berapa perkiraan jumlah barisnya?

Puluhan atau ratusan. Ini menentukan apakah ahli gizi memilih lewat daftar sederhana atau
memerlukan pencarian.

## Pertanyaan 2 — `GIZ-OQ-004` Bentuk kebutuhan nutrisi

**Yang sudah diputuskan.** Modul Gizi berhenti di penentuan diet dan tidak mengurus pemesanan
makanan ke dapur (`GIZ-DEC-004`).

**Yang belum diketahui.** Angka apa saja yang dihitung dan disimpan pada langkah penentuan diet.

### 2a. Zat gizi mana yang dihitung untuk setiap pasien?

| Zat gizi | Dihitung? | Satuan yang dipakai |
|---|---|---|
| Energi | | |
| Protein | | |
| Lemak | | |
| Karbohidrat | | |
| Cairan | | |
| Lainnya, sebutkan | | |

### 2b. Apakah sistem yang menghitung, atau ahli gizi yang mengisi angkanya?

| Pilihan | Akibatnya pada sistem |
|---|---|
| A. Ahli gizi menghitung di luar sistem lalu mengetik hasilnya | Sistem cukup menyediakan kolom angka. Paling sederhana |
| B. Sistem menghitung dari berat, tinggi, umur, dan faktor aktivitas | Rumus dan faktornya harus dilampirkan lengkap. Bila rumusnya salah, seluruh pasien terdampak |
| C. Sistem menghitung tetapi ahli gizi boleh mengubah hasilnya | Perlu menyimpan nilai hitungan dan nilai akhir sekaligus, beserta alasan perubahannya |

Bila jawabannya **B** atau **C**, mohon lampirkan rumus yang dipakai beserta sumbernya. Rumus
tidak akan diambil dari internet.

### 2c. Bentuk diet yang ditetapkan

Apakah diet dipilih dari daftar diet baku rumah sakit, atau ditulis bebas? Bila dari daftar,
mohon lampirkan daftarnya. Ini menentukan apakah dibutuhkan satu master lagi.

### 2d. Apakah kebutuhan nutrisi dapat berubah antar kunjungan?

Bila ya, sistem menyimpan riwayatnya sehingga perubahan dapat ditelusuri. Bila tidak, cukup satu
nilai per episode rawat inap.

## Pertanyaan 3 — `GIZ-OQ-006` Siapa yang berwenang menyetujui

Seluruh keputusan pada `00-interview-decisions.md` tercatat disetujui "Pemilik kebutuhan".
Registry sistem menandai ketiadaan pemilik proses yang jelas sebagai zona konflik `KF-001`.

Mohon disebutkan nama dan jabatan orang yang berwenang menyetujui aturan proses gizi. Tanpa itu,
keputusan modul ini tidak dapat disahkan siapa pun ketika kelak dipersoalkan.

## Yang tidak ditanyakan karena sudah terjawab

Agar tidak menyita waktu pemilik proses, tiga hal berikut sudah dipastikan lewat pemeriksaan
kode dan **tidak perlu dijawab**:

| Hal | Hasil pemeriksaan |
|---|---|
| Skrining gizi awal | Sudah ada di `TrxPatientAssessment` milik Clinical Management. Modul Gizi membacanya, tidak membuat ulang |
| Berat, tinggi, dan BMI | Sudah ada di asesmen pasien yang sama |
| Wadah catatan kunjungan | Memakai CPPT yang sudah ada, dan tempat untuk gizi sudah tersedia di dalamnya |

## Setelah lembar ini dijawab

1. Jawaban 1 dicatat sebagai `GIZ-DEC-011`, jawaban 2 sebagai `GIZ-DEC-012`, lengkap dengan nama
   penyetuju dan tanggalnya.
2. `GIZ-OQ-002`, `GIZ-OQ-004`, dan `GIZ-OQ-006` ditandai tertutup.
3. Kebutuhan dinyatakan `READY`.
4. Desain domain baru dimulai.

Selama lembar ini belum dijawab, tidak ada entity Gizi yang dibuat dan tidak ada migration yang
ditulis.
