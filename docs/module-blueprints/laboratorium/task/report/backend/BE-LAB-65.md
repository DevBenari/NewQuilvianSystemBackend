# Laporan Perubahan Backend — `BE-LAB-65`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-65` (backend) |
| Judul | Permukaan baseline `LabOrganism` dan `LabAntibiotic` — prasyarat `FE-LAB-24` |
| Trace | `rules/backend/master-data-endpoint-standard.md`; `AC-117`, `AC-118`; `VAL-85` |
| Kontrak | `LAB-API-v1` **`r31`** bagian 26 — `approved` 2026-09-22 |
| Klasifikasi | `MEDIUM` — delapan endpoint, satu ruas, dua controller, nol migration |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-22 |
| Dependency | `BE-LAB-44` ✅, `BE-LAB-60` ✅ |
| Status | ✅ **`SELESAI`** — kedelapan endpoint terbukti terhadap `QuilvianNewDevYoga`. **Nol batas diketahui** |

---

## 1. Kenapa task ini ada

`FE-LAB-24` membangun dua layar data induk Mikrobiologi — Organisme dan Antibiotik. Sapuan
menyeluruh atas **seluruh** controller Laboratorium menemukan keduanya berhenti di **empat**
endpoint, sedangkan standar menuntut sembilan (delapan di sini, sebab `DELETE` memang ditolak).

**Ini pengulangan `BE-LAB-64` pada dua grup berikutnya, dan sebabnya sama persis:** `BE-LAB-44`
dibangun ke kontrak `r24` bagian 19.4 yang menyebut empat endpoint, dan
`master-data-endpoint-standard.md` nol dibaca saat itu.

### 1.1 Sapuan menyeluruh, bukan sebagian

Sesudah `BE-LAB-64` saya memeriksa **kedua puluh dua** controller Laboratorium sekaligus alih-alih
menambal satu per satu ketika tersandung. Hasilnya: gapnya **sistemik**, dan tiga grup Patologi
Anatomi memikul gap yang sama persis.

| Grup | Http | Kurang | Menahan |
|---|---|---|---|
| `LabOrganism` | 4 | metadata, summary, `GET /{id}`, `PATCH` | **`FE-LAB-24`** — ditutup task ini |
| `LabAntibiotic` | 4 | keempatnya | **`FE-LAB-24`** — ditutup task ini |
| `LabPathologyCategory` | 6 | keempatnya | `FE-LAB-27` — **masih terbuka** |
| `LabPathologyParameter` | 4 | keempatnya | `FE-LAB-27` — **masih terbuka** |
| `LabProcedurePathologyCategory` | 4 | kelimanya | `FE-LAB-27` — **masih terbuka** |
| `LabDisciplineSetting` | 4 | — varian sah | — |

Ketiga grup Patologi Anatomi **sengaja nol disentuh** task ini: mereka milik `FE-LAB-27`, dan
memasukkannya berarti mengerjakan dua task dalam satu pemanggilan.

### 1.2 `GET /{id}` adalah yang paling mahal di antara keempatnya

Tanpa jalur detail, formulir ubah yang dibuka lewat tautan langsung atau sesudah halaman
disegarkan nol punya cara memuat barisnya — dan **gagalnya diam**: layarnya sekadar tampak
kosong. Modul ini sudah membayar kelas kesalahan itu sekali lewat `r6`, sesudah `FE-LAB-03`
diam-diam gagal di luar halaman daftar. Roadmap frontend revision 33 sudah menuliskannya sebagai
peringatan; di sini ia ditutup.

---

## 2. Yang dibangun

| Berkas | Perubahan |
|---|---|
| `DTOs/LabMicrobiologyMasterDataDtos.cs` | Dua `SummaryResponse`, satu `StatusRequest`, dan **`DiscContentUg` pada tiga DTO antibiotik** |
| `DTOs/LabFilterAndSummaryDtos.cs` | Dua `FilterMetadataResponse`, keduanya `IsDeletable = false` |
| `Services/LabFilterMetadataFactory.cs` | `LabOrganism()`, `LabAntibiotic()` |
| `Services/LabMicrobiologyMasterDataService.cs` | `GetByIdAsync`, `GetSummaryAsync`, `SetStatusAsync` pada kedua service; `DiscContentUg` pada `Map`, `CreateAsync`, `UpdateAsync` antibiotik |
| `Controllers/LabOrganismController.cs` | Empat endpoint; 4 → **8** |
| `Controllers/LabAntibioticController.cs` | Empat endpoint; 4 → **8** |

**Nol migration, nol kolom, nol tabel, nol permission baru.**

---

## 3. Satu kolom yang nol punya jalan diisi — dan itu temuan, bukan permintaan

`LabAntibiotic.DiscContentUg` **sudah ada di tabel sejak `BE-LAB-60`**, tetapi nol satu pun DTO
membawanya. Akibatnya `missingDiscContent` pada ringkasan breakpoint — angka yang **saya sendiri
bangun kemarin lewat `BE-LAB-64`** — adalah angka yang nol dapat diturunkan dari layar mana pun.
Ia melaporkan pekerjaan yang tersisa tanpa menyediakan jalan mengerjakannya.

Ruasnya karena itu dibuka pada respons, create, dan update antibiotik. `null` berarti **belum
diisi**, bukan nol mikrogram.

**Dibuktikan menutup lingkarnya** — lihat bagian 4.

---

## 4. Verifikasi

Seluruhnya lewat HTTPS terhadap `QuilvianNewDevYoga`, 2026-09-22.

| Yang diuji | Hasil | Klasifikasi |
|---|---|---|
| `dotnet build -p:RunAnalyzers=False` | **0 error** | `PASS` |
| Kedelapan endpoint baru | `200` | `PASS` |
| `GET /{id}` dan `PATCH` penunjuk asing | `404` | `PASS` |
| `PUT` antibiotik `discContentUg: 10` | Tersimpan dan terbaca | `PASS` |
| `missingDiscContent` ringkasan **antibiotik** | 1 → **0** | `PASS` |
| `missingDiscContent` ringkasan **breakpoint** | 1 → **0** | `PASS` — **lingkarnya tertutup** |
| Kolom `discContentUg` daftar breakpoint | `null` → **10** | `PASS` |
| `PATCH` organisme → `false` | `activeOrganism` 1→0, `withBreakpoint` 1→**0** | `PASS` |
| `GET /options` sesudahnya | `totalData` **0** | `PASS` — `VAL-85` tegak |
| `GET /` sesudahnya | `totalData` **1** | `PASS` — `AC-118` tegak |
| Seluruhnya dikembalikan | `missingDiscContent` kembali 1, keduanya aktif | `PASS` — **data dev pulih seperti semula** |
| Baris `SysActionAccess` | **3 per controller**, nol baris `Delete` | `PASS` |

**`AUTOMATED TEST: NOT APPLICABLE`** — nol uji unit backend disentuh; verifikasinya pemanggilan
HTTP terhadap database sungguhan (`LAB-RDY-C04`).

**`MANUAL TEST: PASS`** — kedelapan endpoint dipanggil satu per satu beserta jalur gagalnya.

### 4.1 Dua cacat ditemukan saat menguji, dan diperbaiki

**`withBreakpoint` membantah dirinya sendiri.** Versi pertama menghitung breakpoint aktif tanpa
memeriksa apakah organismenya sendiri masih aktif, sehingga ringkasannya berbunyi *"nol organisme
aktif, tetapi satu tercakup"*. Kini kedua syarat diperiksa, dan dibuktikan `withBreakpoint` ikut
turun ke `0` ketika organismenya dinonaktifkan.

**Pesan jawabannya berbahasa Inggris.** `PATCH` menjawab *"Lab Organism dinonaktifkan."* — nama
teknis controller, bukan kalimat yang dibaca petugas. Diganti menjadi *"Organisme
dinonaktifkan."* dan *"Antibiotik dinonaktifkan."*

---

## 5. Yang sengaja TIDAK dikerjakan

| Butir | Alasan |
|---|---|
| `DELETE` pada kedua grup | `r24` 19.4 menolaknya atas alasan klinis; `AC-117` menuntut layarnya nol punya tombol Hapus. `isDeletable: false` pada metadata menyatakannya, supaya layar membacanya alih-alih menyimpulkan dari ada-tidaknya endpoint |
| Tiga grup data induk Patologi Anatomi | Milik `FE-LAB-27`. Gapnya **dicatat** pada bagian 1.1, bukan ditambal diam-diam |
| Mengisi data induknya | `B3` pada audit kesiapan. Task ini membuka **jalannya**; mengisinya pekerjaan kepala instalasi lewat `FE-LAB-24` |
| Nama aksi tersendiri bagi endpoint baru | Akan melahirkan baris hak akses yang nol tercentang pada role mana pun, sehingga layar `FE-LAB-24` menjawab `403` sampai seseorang membukanya satu per satu |

---

## 6. Koreksi status yang dibawa serta

Kontrak bagian 19.4 menandai kedelapan endpoint lama **`Rencana (belum tersedia)`** padahal
`BE-LAB-44` membangunnya 2026-09-18. Kedelapannya dipanggil, kedelapannya menjawab `200`, dan
kolom statusnya dikoreksi menjadi **`Tersedia`**. Yang berubah hanya kolom status.

---

## 7. Akibatnya bagi `FE-LAB-24`

**Penahannya hilang.** Kedua layar kini berdiri di atas delapan endpoint penuh dan dapat dibangun
persis sesuai `master-data-feature-standard.md`. Sesudah keduanya berdiri, `B3` dapat ditutup
dari aplikasi — bukan lewat SQL langsung ke basis data.

**Nol operasi git dijalankan.**
