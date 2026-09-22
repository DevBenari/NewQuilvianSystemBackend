# `LAB-EVD-005` — Cetakan hasil tiga disiplin (2026-09-21)

| Field | Nilai |
|---|---|
| Evidence ID | `LAB-EVD-005` |
| Diserahkan | Pemilik modul, 2026-09-21 |
| Bentuk | Dua berkas PDF dan satu foto dokumen tercetak |
| Menutup sebagian | `LAB-OPEN-039` |
| Melahirkan | `LAB-DEC-114` sampai `LAB-DEC-121` (amendment pass putaran 11) |

## 1. Berkas yang diproses

| Berkas | Disiplin | Kasus |
|---|---|---|
| `mikrobiologi (1).pdf` | Mikrobiologi | **Kultur jamur** — `JAM KUL LAIN-LAIN KULTUR JAMUR & RES` |
| `patalogi anatomi (1).pdf` | Patologi Anatomi | Kategori **Histological** |
| Foto dokumen tercetak | Patologi Klinik | **Rutin** — Hematologi |

## 2. Cetakan Mikrobiologi — isi yang terbaca

### Kop

Logo rumah sakit di kiri; identitas **Rumah Sakit Metropolitan Medical Centre** beserta alamat,
telepon, faks, nomor IGD, surel, dan laman di tengah; lambang **Terakreditasi Paripurna KARS**
di kanan. Judul **LABORATORIUM MIKROBIOLOGI** di tengah.

### Blok identitas — dua kolom

| Kolom kiri | Kolom kanan |
|---|---|
| No. Lab : `26-1129` | No. Reg. : `2607290425` |
| Nama Pasien *(nama + gelar + sapaan + jenis kelamin dalam satu baris)* | No. Rekam Medik : `00-56-41-72` |
| Unit : `RAWAT INAP, (VVIP/ VVIP 907)` — **unit beserta kamarnya** | Usia : `44 Tahun` |
| Nama Dokter | Jenis Kelamin |
| Bahan : `Throat Specimens` — **berbahasa Inggris** | Tanggal Terima : `03 Agustus 2026` |
| Pemeriksaan : `JAM KUL LAIN-LAIN KULTUR JAMUR & RES` | Tanggal Selesai : `07 Agustus 2026` |

### Badan hasil

```text
HASIL YANG DIPEROLEH : DEFINITIF

HASIL BIAKAN JAMUR : Candida albicans.

HASIL RESISTENSI ANTIJAMUR :
- Nystatin 1,25 ug/mL      ( Sensitive)
- Amphotericin 2 ug/mL     ( Sensitive)
...sepuluh baris seluruhnya
```

Tiga hal yang perlu diperhatikan dari bentuk ini:

1. **`DEFINITIF` dicetak sebagai baris tersendiri yang menonjol** — ia kualifikasi hasil, bukan
   catatan sampingan.
2. **Label bagian menyebut JAMUR, bukan bakteri** — `BIAKAN JAMUR` dan `RESISTENSI ANTIJAMUR`.
3. **Setiap nilai MIC membawa satuannya** — `1,25 ug/mL`, bukan `1,25` telanjang. Interpretasi
   ditulis sebagai kata Inggris penuh `( Sensitive)`, bukan huruf `S`.

### Footer

Kalimat anjuran menghubungi kembali dokter peminta; area **Catatan :**; lalu kotak dua kolom —
kiri **Konsultan Mikrobiologi Klinik** beserta nama `Usman Chatib Warsa, PhD, SpMK-K, Prof. dr.`,
kanan atas **Tgl. Cetak** dan kanan bawah **Petugas Otorisasi**.

## 3. Cetakan Patologi Anatomi — yang berbeda dari Mikrobiologi

Judul **dua baris**: `PATOLOGI ANATOMIK` lalu `HASIL PEMERIKSAAN HISTOLOGICAL` — baris kedua
mengikuti kategori.

Blok identitasnya **bertabel penuh**, bukan dua kolom polos, dan memuat ruas yang **tidak ada**
pada Mikrobiologi: `Tanggal Order`, `RS. Rujukan`, `Jenis Spesimen`, `Lokasi Spesimen`
(bernomor, lebih dari satu), `Keterangan Klinik`, `Penyakit yang Diperkirakan`,
`Pemeriksaan sebelumnya bila ada`, dan **`Fiksasi`** (`Formalin buffer 10%`).

Badan hasilnya tiga bagian bernomor Romawi: **Makroskopik**, **Mikroskopik**, **Kesimpulan**.

Footernya **berbeda**: nol kalimat anjuran, nol area Catatan. Hanya kotak dua kolom berisi
**Spesialis Patologi Anatomi** beserta nama, **Tgl. Cetak**, dan **Petugas Otorisasi**.

## 4. Cetakan Patologi Klinik — yang berbeda dari keduanya

Bentuknya **sama sekali lain**: bergaya cetak matriks titik dengan huruf berjarak tetap.

Yang hanya muncul di sini:

- **Kode QR** di kanan atas kop.
- Baris **`Konsultan: Prof.Dr.Riadi Wirawan SpPK(K)`** di bawah kop, di atas blok identitas.
- **`Halaman : 1 / 1`** — penomoran halaman.
- Tiga nomor sekaligus: `NO.LAB.`, `NO. TRANS.`, dan `NO. MUTASI`.
- **Alamat pasien** dicetak.
- Tabel hasil berkolom **JENIS PEMERIKSAAN | HASIL | SATUAN | NILAI RUJUKAN**, berkelompok
  (`HEMATOLOGI` → `Hematologi rutin` → butir, dengan subkelompok `Nilai eritrosit rerata`).
- **Penanda `H`** mendahului nilai di atas rujukan — `H 16.3`, `H 37.7`.
- **Dua baris tanda tangan terpisah: `Otorisasi oleh :` dan `Validasi oleh :`**, beserta stempel
  rumah sakit dan tanda tangan dokter.

> **Dua baris tanda tangan itu bukti lapangan langsung untuk prinsip empat mata `LAB-DEC-003`.**
> Laboratorium ini sudah membedakan pemvalidasi dari pengotorisasi pada dokumen yang dicetak
> hari ini — dan itu menguatkan `LAB-DEC-097` yang menetapkan `Simpan Final` **bukan** rilis.

## 5. Pertentangan yang dibuka bukti ini

| # | Temuan | Bertentangan dengan | Ditutup oleh |
|---|---|---|---|
| 1 | `DEFINITIF` dicetak sebagai kualifikasi hasil | `LAB-DEC-106` — Definitif hanya fakta konsultasi | `LAB-DEC-114` |
| 2 | MIC membawa satuan `ug/mL` | `LAB-API-v1` `r26` — `Concentration` nol satuan | `LAB-DEC-115` |
| 3 | Biakan **jamur** dan **antijamur** | Seluruh `S4b` dimodelkan untuk bakteri | `LAB-DEC-116` |
| 4 | Nomor cetak per disiplin per tahun | `LAB-DEC-072` — satu `OrderNumber` global | `LAB-DEC-117` |
| 5 | `Tanggal Terima` bukan waktu pengambilan bahan | `LAB-DEC-096` — Waktu Efektif dari `CollectedAt` | `LAB-DEC-118` |
| 6 | Konsultan bukan `DR-LAB-002` | Dugaan bahwa keduanya orang yang sama | `LAB-DEC-119` |
| 7 | `Petugas Otorisasi` sebagai peran tersendiri | Belum pernah dipetakan | `LAB-DEC-120` |
| 8 | Konsultan bergelar **Prof.** | `LAB-OPEN-029` — nol nama bergelar Profesor pada penetapan | `LAB-DEC-121` mengajukannya, **bukan** menutupnya |

## 6. Yang masih kurang sesudah bukti ini

`LAB-OPEN-039` **belum tertutup penuh.** Yang masih dibutuhkan:

**Mikrobiologi — lima varian:**

1. Versi **bakteri** (`BIAKAN BAKTERI` / `RESISTENSI ANTIBIOTIKA`).
2. Versi dengan **zona hambat dalam mm** — contoh ini hanya memakai MIC.
3. Versi **lebih dari satu isolat** — susunannya belum terlihat.
4. Versi **kultur steril / nol pertumbuhan** — kalimat apa yang tercetak.
5. Versi **halaman kedua** — `LAB-DEC-110` mewajibkan kop dan identitas berulang; ketiga contoh
   ini satu halaman semua.

**Patologi Anatomi — tiga kategori:** Sitologi, IHK, dan FNAB. Juga cara **gambar** dicetak
(`DEC-LAB-016`).

**Lainnya:** Nota Lab, Label Lab, dan Label Golongan Darah (`LAB-OPEN-032`); serta cetakan
Bahasa Inggris (`LAB-COORD-013`).

---

# `LAB-EVD-006` — Cetakan Mikrobiologi varian bakteri (2026-09-21)

Dua tangkapan layar halaman cetak `LABORATORIUM MIKROBIOLOGI`, pemeriksaan
`MO KUL SPUTUM KULTUR MO & RES`, No. Lab `26-1246`. **Ini varian bakteri yang `LAB-OPEN-039`
catat sebagai kurang.** Melahirkan `LAB-DEC-122` sampai `LAB-DEC-128`.

## Bentuk yang terbaca

Label organisme: **`IDENTITAS : Branhamella catarrhalis`** — **bukan** `HASIL BIAKAN BAKTERI`
seperti dugaan `LAB-DEC-116`. Di atas tabel ada keterangan baku
`Kode : R = Resistant, I = Intermediate, S = Sensitive`.

Tabel **lima kolom**: `ANTIBIOTIK | UG | R-S | Zona / mm | RESULT`, 22 baris.

| ANTIBIOTIK | UG | R-S | Zona | RESULT |
|---|---:|---|---:|---|
| AMPICILLIN | 10 | 13 - 17 | 20 | S |
| CEFOPERAZONE+ SULBACTAM | 105 | 15 - 21 | 24 | S |
| NETILMICIN | 30 | 12 - 15 | 13 | **I** |
| FOSFOMYCIN | 200 | 12 - 16 | 11 | R |
| GENTAMICIN | 10 | 12 - 15 | **0** | R |
| MEROPENEM | 10 | 13 - 16 | 31 | S |

**Seluruh 22 baris konsisten** terhadap satu aturan: zona di bawah batas bawah menghasilkan
`R`, di dalam rentang menghasilkan `I`, di atas batas atas menghasilkan `S`. **Sebelas baris
bernilai zona `0`, dan seluruhnya `R`.**

Blok identitasnya juga berbeda dari contoh jamur: usia ditulis `58 thn. 10 bln. 16 hr.`, dan
`Tgl. Terima` serta `Tgl. Selesai` **menyertakan jam** — `28 Agustus 2026 / 16:10` dan
`31 Agustus 2026 / 11:01`.

Penutupnya: `HASIL YANG DIPEROLEH : DEFINITIF`, lalu **CATATAN** berisi dua hal — kalimat baku
`LEBAR ZONA ANTIBIOTIK TIDAK MEMPENGARUHI TINGKAT KEPEKAAN BAKTERI.` dan catatan khas pasien
yang menyebut **dua kuman** sekaligus, padahal hanya satu yang bertabel.

## Pertentangan yang dibuka bukti ini

| Temuan | Bertentangan dengan | Ditutup oleh |
|---|---|---|
| Kolom `UG` dan rentang `R-S` | `r26` nol mengenal keduanya | `LAB-DEC-122` |
| Interpretasi dapat dihitung dari zona | `r26` mewajibkan analis mengetiknya | `LAB-DEC-123` |
| Bentuk bakteri dan jamur berbeda total | `LAB-DEC-116` menduga hanya label | `LAB-DEC-124` |
| Tidak semua pemeriksaan memakai set bakteri | Katalog nol punya penandanya | `LAB-DEC-125` |
| Kuman kedua hanya muncul di Catatan | Belum pernah dimodelkan | `LAB-DEC-126` |
| Kalimat baku pada Catatan | Belum pernah dimodelkan | `LAB-DEC-127` |
| Zona `0` sebagai data | Belum pernah dinyatakan | `LAB-DEC-128` |

## Yang MASIH kurang sesudah bukti ini

`LAB-OPEN-039` **tetap belum tertutup penuh:**

1. Mikrobiologi dengan **lebih dari satu isolat yang sama-sama berantibiogram** — contoh ini
   hanya punya satu tabel `IDENTITAS`.
2. Mikrobiologi **kultur steril / nol pertumbuhan**.
3. Mikrobiologi **halaman kedua**.
4. Patologi Anatomi kategori **Sitologi**, **IHK**, dan **FNAB**, beserta cara gambar dicetak.
5. Nota Lab, Label Lab, dan Label Golongan Darah (`LAB-OPEN-032`).
6. Cetakan Bahasa Inggris (`LAB-COORD-013`).
