# `LAB-REQ-006` — Persetujuan penambahan bagian Laboratorium pada kiosk

| Field | Nilai |
| --- | --- |
| Request ID | `LAB-REQ-006` |
| Tanggal | 2026-09-15 |
| Diajukan oleh | Yoga Aji Pratama (`yogaaji452@gmail.com`), pemilik modul Laboratorium |
| Ditujukan kepada | Pemilik `registration-management` |
| Status | **`disetujui`** |
| Disetujui oleh | **Andry Zain** (`andryzainhome`) |
| Cara penyampaian | **Lisan, diteruskan pemilik modul** pada sesi 2026-09-15. Dokumen ini ditulis sesudahnya sebagai pembukuan |
| Menutup | `LAB-COORD-008`, `LAB-COORD-009` |
| Melepas | `LAB-DEC-051`, `LAB-DEC-052`, `LAB-DEC-053`, `LAB-DEC-054` |

> **Cara pembukuan yang harus jujur.** Persetujuan ini **tidak** diberikan langsung dalam bentuk
> tertulis pada berkas ini; ia disampaikan lisan kepada pemilik modul Laboratorium, lalu
> diteruskan. Dicatat apa adanya. Bila pemilik `registration-management` kelak membaca dan
> menemukan cakupannya berbeda dari yang tertulis di sini, **dokumen inilah yang keliru**, bukan
> pekerjaannya — dan koreksinya wajib dicatat sebagai revisi, bukan diperbaiki diam-diam.
>
> Presedennya sudah ada: `LAB-COORD-004` yang juga menyangkut `registration-management` ditutup
> oleh `andryzainhome` dan `sukmagp` lewat `LAB-REQ-001` pada 2026-09-01.

---

## 1. Yang disetujui

### 1.1 Kiosk bertambah bagian Laboratorium

Pasien dapat memilih **Laboratorium** sebagai layanan yang dituju langsung dari kiosk. Hari ini
`TrxKioskScanSession` hanya memindai identitas dan mencocokkannya ke `PatientId`; ia tidak
mengetahui layanan yang dituju pasien.

**Yang perlu ditambahkan pada milik `registration-management`:**

| Butir | Isi |
| --- | --- |
| Tujuan layanan | Sesi kiosk membawa layanan yang dipilih pasien, dengan Laboratorium sebagai salah satu nilainya |
| Jalur permintaan | Sesi kiosk membedakan pasien yang **membawa permintaan dokter** dari pasien yang **memeriksakan diri sendiri** (`LAB-DEC-052`) |

Keduanya **aditif**. Tidak satu pun ruas, nilai, atau perilaku `TrxKioskScanSession` yang sudah
ada boleh berubah, berganti nama, atau hilang — 16 sesi nyata sudah tersimpan pada tabel itu,
15 di antaranya sudah cocok ke pasien.

### 1.2 Kunjungan terbentuk di kiosk

Kunjungan (`TrxPatientEncounter`) terbentuk **begitu pasien selesai di kiosk**, bukan menunggu
petugas laboratorium memproses (`LAB-DEC-053`). Pembentukannya tetap sepenuhnya dikerjakan
Registrasi; `AC-45` melarang Laboratorium membentuk maupun mengubah kunjungan, dan larangan itu
**tidak dicabut** oleh persetujuan ini.

### 1.3 Kebijakan kedaluwarsa — butir yang paling berkonsekuensi

Pasien yang memilih Laboratorium di kiosk lalu pergi tanpa diperiksa meninggalkan kunjungan
tanpa pemeriksaan. Ketentuannya (`LAB-DEC-058`):

| Butir | Ketentuan |
| --- | --- |
| Kapan ditutup | **Saat hari layanan berakhir**, otomatis oleh Registrasi, dengan sebab "tidak dilanjutkan" |
| Biaya pendaftaran | **Gugur** bersama kunjungannya. Pasien yang batal tidak menanggung apa pun |
| Siapa yang menutup | **Registrasi**, bukan Laboratorium |

**Kenapa biaya digugurkan, dan kenapa itu perlu ditulis terang.** Pasien yang batal tidak
menerima satu pun layanan klinis. Menagihnya biaya pendaftaran berarti menagih orang yang tidak
mendapat apa-apa, dan penagihan seperti itu paling sering terbongkar di meja kasir — di depan
pasien lain. Ketentuan ini menutup kemungkinan itu sebelum sempat terjadi.

**Pasien yang batal tidak menghasilkan tagihan pemeriksaan laboratorium** dalam keadaan apa pun.
Fakta kelayakan tagih baru terbit ketika wadah dinyatakan layak (`AC-37`), dan pasien yang tidak
pernah sampai ke meja lab tidak punya wadah.

---

## 2. Yang **tidak** termasuk dalam persetujuan ini

Ditulis eksplisit supaya tidak terbaca lebih luas daripada yang diberikan:

| Hal | Keadaan |
| --- | --- |
| Data induk instansi perujuk | Tetap tertahan `LAB-COORD-006`. Jalur "bawa permintaan dokter **luar**" belum dapat dipakai penuh sampai data induknya punya endpoint tulis |
| Nilai `EncounterPaymentType` untuk piutang mitra | Tetap tertahan `LAB-COORD-007` |
| `FR-11.9` dan `FR-11.10` | Tetap `OPEN DECISION` di bawah `LAB-REQ-005` |
| Kewenangan Laboratorium atas kunjungan | **Tidak berubah.** `AC-45` tetap berlaku penuh |

---

## 3. Akibat pada pekerjaan

| Penahan | Keadaan sesudah persetujuan ini |
| --- | --- |
| `LAB-COORD-008` | **Ditutup** |
| `LAB-COORD-009` | **Ditutup** oleh `LAB-DEC-058` |
| `LAB-DEC-051`, `LAB-DEC-052` | `draft` → **`approved`** |
| `LAB-DEC-053`, `LAB-DEC-054` | `draft` → **`approved`** |

Perubahan pada milik `registration-management` dikerjakan mengikuti pola `BE-EXT-01` sampai
`BE-EXT-03`: task berada pada roadmap Laboratorium, diberi awalan `BE-EXT`, dan dikerjakan atas
wewenang persetujuan lintas modul ini — bukan atas asumsi kepemilikan.
