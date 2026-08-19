# Laporan Perubahan Backend — `BE-IGD-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-013` |
| Judul | Penyimpangan struktur folder IGD dirapikan |
| Slice | S5 — Utang teknis dan kesiapan |
| Trace | `02-backend-architecture.md` bagian 4 "Utang teknis"; `DEC-RSK-003` |
| Commit backend | `dd97c6e` |
| Tanggal | 18 Agustus 2026 |
| **Status** | **Dua dari tiga bagian selesai — belum dikompilasi; bagian ketiga ditahan karena ambigu** |

---

## 1. Yang dikerjakan

### 1.1 `Controller/` menjadi `Controllers/`

Sembilan controller IGD dipindahkan. **Nol perubahan kode**: seluruh berkas itu sudah
mendeklarasikan `namespace ...EmergencyInstallationManagement.Controllers` sejak awal, sehingga
yang menyimpang hanya nama foldernya. Git mencatatnya sebagai rename murni.

### 1.2 `Configurations/HealthService/` menjadi `Configurations/HealthServices/`

83 berkas konfigurasi EF dipindahkan, dan namespace-nya diselaraskan:

| Bentuk namespace | Jumlah berkas |
| --- | ---: |
| `...Configurations.HealthServices` | 67 |
| `...Configurations.HealthServices.EmergencyInstallationManagement` | 9 |
| `...Configurations.HealthServices.MasterData.EmergencyInstallationManagement` | 6 |
| `...Areas.HealthServices.ClinicalManagement.Configurations` (berbeda sejak awal) | 1 |

Perubahan ini aman karena **nol berkas di luar folder tersebut merujuk namespace itu**. EF
menemukan konfigurasi lewat pemindaian assembly, bukan lewat `using` eksplisit.

## 2. Yang ditahan: penyelarasan namespace master IGD

Bagian ketiga pada scope roadmap berbunyi "namespace master IGD diselaraskan dengan foldernya".
Kenyataannya 49 berkas master IGD berada di `Areas/HealthServices/MasterData/{Controllers,DTOs,
Models,Seeders,Services}/` tetapi mendeklarasikan
`...MasterData.EmergencyInstallationManagement.{Controllers,DTOs,Models,...}`.

Ada dua cara menyelaraskannya, dan keduanya sah menurut kalimat roadmap:

| Cara | Akibat |
| --- | --- |
| **A.** Ubah namespace mengikuti folder | 49 berkas berubah namespace, dan berkas di luar master ikut berubah karena `using`-nya patah. Tipe master IGD melebur dengan master lain dalam satu namespace |
| **B.** Pindahkan berkas mengikuti namespace, ke `MasterData/EmergencyInstallationManagement/` | Nol perubahan kode, hanya perpindahan berkas. Struktur folder master IGD menjadi berbeda dari master lain |

Cara A menyentuh berkas di luar modul IGD; cara B tidak. Tetapi cara B membuat master IGD
menjadi satu-satunya master yang punya sub-folder modul, yang justru bisa dibaca sebagai
penyimpangan baru.

Roadmap tidak menyatakan mana yang dimaksud, dan `DoD` mensyaratkan "nol perubahan perilaku"
yang hanya dapat dibuktikan dengan build serta regression. Karena keduanya tidak tersedia,
bagian ini saya tahan dan bukan saya tebak. Owner: Backend/API.

## 3. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| Referensi namespace lama tersisa di seluruh repo | **Nol** |
| Referensi ke `Configurations.HealthService` dari luar folder sebelum rename | **Nol** |
| Git mendeteksi perpindahan sebagai rename, bukan hapus-tambah | Ya |
| Folder lama `Controller/` dan `HealthService/` sudah tidak ada | Ya |
| Build | **Tidak dijalankan** atas permintaan owner |
| Regression test | **Tidak ada** — solution tidak memiliki test project |

### 3.1 Satu verifikasi yang sempat salah dan diperbaiki

Pemeriksaan pertama memakai `grep -v "HealthServices"` untuk menyaring hasil. Penyaring itu
ikut membuang baris berdasarkan **path** berkas, yang setelah rename memang mengandung
"HealthServices". Akibatnya 67 berkas yang masih memakai namespace lama tidak terlihat dan
sempat dilaporkan bersih.

Verifikasi ulang memakai `grep -rh` sehingga hanya isi baris yang dibaca. Ketujuh puluh tujuh
berkas kemudian diselaraskan seluruhnya. Dicatat di sini karena pemeriksaan yang salah lebih
berbahaya daripada pemeriksaan yang tidak dilakukan.

## 4. Risiko tersisa

| No | Risiko | Penanganan |
| ---: | --- | --- |
| 1 | Rename dilakukan sebelum build hijau | Bila build gagal, sebagian besar berkas berubah lokasinya sehingga galat lebih sulit ditelusuri. Build wajib dijalankan sebelum perubahan ini digabung |
| 2 | Bagian ketiga masih menyimpang | Tercatat terbuka; arsitektur bagian 4 belum boleh dinyatakan lunas |
| 3 | Konflik merge dengan pekerjaan paralel | Rename folder menghasilkan konflik yang lebar; sebaiknya digabung lebih dulu sebelum orang lain menyentuh modul IGD |

## 5. Dampak pada dokumen

`02-backend-architecture.md` bagian 4 **belum** boleh dinyatakan lunas, karena satu dari tiga
penyimpangan masih terbuka.
