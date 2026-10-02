# Human Resource — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Module name | `Human Resource` |
| Module slug | `human-resource` |
| Revision | `9` |
| Module status | `PARTIAL` |
| Current phase | `HRD-PH-SECURITY-CLOSED` — seluruh blocker MVP dan co-sign keamanan selesai; menunggu approval akhir manusia |
| Last verified at | `2026-08-30` — impact scan read-only terhadap HEAD kedua repository, bukan verifikasi runtime |
| Backend source SHA — diaudit pada | `ecdc135444f0110482c9702212bcea30043983c8` (branch `AndryZain`) — **historical**, dipertahankan sebagai provenance audit lama |
| Backend source SHA — diverifikasi berlaku pada | `16b8b71f4cd61e083213cf90722f4d768d339739` (`origin/QuilvianIntegrationBackend`, baseline canonical) |
| Backend source SHA — HEAD saat desain ditulis | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` (branch kerja `AndryZain`) |
| Frontend source SHA | `fff76a1b394d4b247c70a04f106c8ec098c9696e` (branch `AgentCodexFrontend`) |
| Frontend source SHA sebelumnya | `2a1cea7841a4433f8637d486204e60314c09d131` |

Status modul `PARTIAL` dipilih sesuai kontrak status pada template: sekurang-kurangnya satu fase
siap dijalankan sementara fase lain terblokir. Itu persis keadaan modul ini.

---

## 1. Klasifikasi kesiapan desain

Empat nilai berikut dipakai konsisten di seluruh blueprint HR. Nilai ini menjawab satu
pertanyaan: **apakah bagian ini boleh dirancang sekarang.**

| Nilai | Artinya | Syarat naik status |
| --- | --- | --- |
| `READY FOR DESIGN` | Seluruh keputusan yang dibutuhkan sudah ada. Desain boleh disusun sampai final | — |
| `PARTIAL` | Sebagian boleh dirancang final, sebagian lagi tidak. Batas antara keduanya wajib disebut eksplisit | Bagian yang tertahan menunggu keputusan yang disebut namanya |
| `BLOCKED` | Tidak boleh dirancang sama sekali. Merancangnya berarti mengarang kewenangan yang belum ada | Dependency terpenuhi **dan** buktinya dicatat |
| `DEFERRED` | Boleh dirancang secara teknis, tetapi sengaja ditunda karena prioritas, bukan karena terhalang | Keputusan pemilik untuk menjadwalkannya |

**Aturan yang mengikat:** `BLOCKED` tidak boleh berubah menjadi `READY FOR DESIGN` berdasarkan
asumsi, kemiripan dengan modul lain, atau karena pekerjaannya terasa mendesak. Kenaikan status
hanya sah bila dependency yang disebut pada tabel bagian 3 benar-benar terpenuhi dan buktinya
tercatat.

Perbedaan `BLOCKED` dan `DEFERRED` sering tertukar, padahal akibatnya berbeda jauh:

> Kredensial klinis berstatus `BLOCKED`. Bukan karena tidak penting, melainkan karena batas
> keselamatan klinisnya belum ditetapkan pihak yang berwenang. Merancangnya sekarang berarti
> menebak kewenangan praktik dokter.
>
> Perjalanan dinas dan reimbursement berstatus `DEFERRED`. Tidak ada yang menghalangi, tetapi
> nilainya paling kecil dibanding slice lain, jadi sengaja diletakkan belakangan.

---

## 2. Kesiapan per kelompok kemampuan

| Kelompok kemampuan | Slice | Kesiapan desain | Alasan |
| --- | --- | --- | --- |
| Fondasi registry dan penamaan route | `S0-A`, `S0-B` | `READY FOR DESIGN` | Keputusan `HRD-DEC-016` dan `HRD-DEC-019` sudah `approved` |
| Administrasi kepegawaian | `S-A1` | `READY FOR DESIGN` | Kontrak backend sudah ada dan sudah dipakai; `HRD-DEC-012` sudah `approved` |
| Layanan mandiri pegawai | `S-A2` s.d. `S-A6` | `READY FOR DESIGN` | 13 controller layanan mandiri sudah ada; `HRD-DEC-007` mengunci konvensi route |
| Kotak masuk persetujuan terpadu | `S-A7` | `READY FOR DESIGN` | `HRD-DEC-011` dan `HRD-DEC-018` sudah `approved` |
| Administrasi kehadiran | `S-B1` | `READY FOR DESIGN` | 71 endpoint sudah ada termasuk tutup dan buka periode |
| Administrasi cuti dan saldo | `S-B2` | `READY FOR DESIGN` | 93 endpoint sudah ada |
| Administrasi lembur | `S-B3` | `READY FOR DESIGN` | 78 endpoint sudah ada |
| Penjadwalan kerja | `S-B4` | `READY FOR DESIGN` | Backend tipis, tetapi arah `EXTEND`-nya jelas dan tidak menyentuh keputusan terbuka |
| Payroll sisi HR | `S-B5` | `PARTIAL` | Batas tanggung jawab final lewat `HRD-DEC-009`; **bentuk serah terima ke Finance belum**, menunggu `HRD-Q-10` dan `HRD-Q-11` |
| Kompetensi dan pelatihan | `S-C2` | `READY FOR DESIGN` | Administratif, tidak menyentuh kewenangan klinis |
| Manajemen kinerja | `S-C3` | `READY FOR DESIGN` | Sama |
| Lifecycle dan offboarding | `S-C4` | `READY FOR DESIGN` | Sama |
| Hubungan karyawan dan kedisiplinan | `S-C5` | `READY FOR DESIGN` | Sama |
| Kredensial, kewenangan klinis, SPK/RKK | `S-C1` | `BLOCKED` | Menunggu `requirement-completeness-gate` dan `hospital-domain-architect`; rilis menunggu `HRD-Q-08` Komite Medik |
| OPPE dan FPPE | bagian `S-C1` | `BLOCKED` | Sama, ditambah belum ada satu pun entity maupun endpoint hari ini |
| Kesehatan dan keselamatan kerja staf | `S-C6` | `BLOCKED` | Menunggu kedua skill hulu, ditambah pengesahan K3RS atas `HRD-DEC-010` |
| Perencanaan tenaga kerja | `S-D1` | `BLOCKED` | Penurunan ulang **model data** tidak boleh berjalan sebelum `HRD-Q-05` dijawab. Istilah "ERD" pada `HRD-DEC-004` bermakna penurunan model data, **bukan** folder `erd/` — folder itu tidak dipakai kontrak keluaran terbaru |
| Rekrutmen dan hiring | `S-D2` | `BLOCKED` | Sama |
| Benefit | `S-D3` | `BLOCKED` | Sama |
| Layanan HR dan tiket kepegawaian | `S-D4` | `BLOCKED` | Sama |
| Perjalanan dinas dan reimbursement | `S-D5` | `DEFERRED` | Secara teknis sama dengan `S-D1` s.d. `S-D4`, tetapi prioritasnya paling rendah. Tetap tidak boleh jalan sebelum `HRD-Q-05` |
| Ratchet penamaan `Trx` saat disentuh | `S-E` | `READY FOR DESIGN` | `HRD-DEC-019` menetapkannya sebagai **aturan lintas-slice**, bukan kampanye yang dijadwalkan. `Wfp` dan `Mst` tidak diubah |

Rekapitulasi **kelompok kemampuan**: **15 `READY FOR DESIGN`**, **1 `PARTIAL`**, **6 `BLOCKED`**,
**1 `DEFERRED`**.

Angka ini menghitung **kelompok kemampuan**, bukan slice implementasi. Roadmap menghitung 26
slice dengan sebaran **18 `READY`**, **1 `PARTIAL`**, **6 `BLOCKED`**, dan **1 `DEFERRED`**.
Keduanya memang berbeda dan tidak perlu disamakan: satu kelompok kemampuan dapat memuat lebih
dari satu slice, misalnya "Layanan mandiri pegawai" mencakup lima slice `S-A2` sampai `S-A6`.

---

## 3. Blocker dan pemiliknya

| Blocker ID | Ringkasan | Pemilik | Fase terdampak | Fase lain boleh jalan? |
| --- | --- | --- | --- | --- |
| `HRD-BLK-001` | Batas keselamatan klinis untuk kredensial dan kewenangan klinis belum ditetapkan pihak berwenang | `requirement-completeness-gate` lalu `hospital-domain-architect`, lalu Komite Medik | `HRD-PH-005` | **Ya.** Seluruh fase administratif tidak bergantung padanya |
| `HRD-BLK-002` | Aturan akses rekam kesehatan kerja belum disahkan | Kedua skill hulu, lalu K3RS | `HRD-PH-006` | **Ya** |
| `HRD-BLK-003` | Bentuk data serah terima payroll dan perilaku saat Finance menolak batch belum disepakati | Pemilik produk bersama Finance | **`POST-MVP` saja** sejak `HRD-DEC-035` | **Ya.** Orkestrasi putaran payroll keluar dari jalur kritis MVP; kesiapan payroll sisi HR tetap di dalam MVP |
| `HRD-BLK-004` | Isi tabel 67 entity yang benar-benar belum punya API belum diketahui, sehingga keputusan skema yang merusak data tidak boleh diambil | Pemilik database | `HRD-PH-007` | **Ya** |
| `HRD-BLK-005` | Nama pemilik kebijakan bisnis, wakil Komite Medik, dan wakil K3RS belum ada | Manajemen | Approval seluruh blueprint | **Ya** untuk desain; **tidak** untuk rilis produksi slice sensitif |

Tidak ada satu pun blocker di atas yang menghentikan fase administratif. Itu sebabnya status
modul `PARTIAL`, bukan `BLOCKED`.

---

## 4. Keadaan fase

| Fase | Isi | Status | Slice |
| --- | --- | --- | --- |
| `HRD-PH-001` | Fondasi: registry dan penamaan route | `READY` | `S0-A`, `S0-B` |
| `HRD-PH-002` | Layanan mandiri pegawai dan kotak masuk persetujuan | `READY` | `S-A1` s.d. `S-A7` |
| `HRD-PH-003` | Administrasi waktu kerja: kehadiran, cuti, lembur, penjadwalan | `READY` | `S-B1` s.d. `S-B4` |
| `HRD-PH-004` | Payroll sisi HR | `READY` sampai perhitungan; serah terima `BLOCKED` | `S-B5` |
| `HRD-PH-005` | Kredensial dan kewenangan klinis | `BLOCKED` | `S-C1` |
| `HRD-PH-006` | Kesehatan dan keselamatan kerja staf | `BLOCKED` | `S-C6` |
| `HRD-PH-007` | Domain tanpa API yang diturunkan ulang | `BLOCKED` | `S-D1` s.d. `S-D5` |
| `HRD-PH-008` | Pengembangan orang: kompetensi, kinerja, lifecycle, hubungan karyawan | `READY` | `S-C2` s.d. `S-C5` |
| `HRD-PH-009` | Ratchet penamaan `Trx` saat entity disentuh | **Bukan fase terjadwal.** Aturan lintas-slice yang berlaku sepanjang implementasi | `S-E` |
| `HRD-PH-DESIGN-COMPLETE` | Penyusunan ketiga belas artefak canonical `design-business-module` | **`DONE` sebagai `draft`.** Seluruh artefak ada; approval manusia belum ada dan **tidak** diklaim skill | seluruh slice `READY` dan `PARTIAL` |

Fase yang terblokir **tidak** menghalangi fase `READY` yang tidak bergantung padanya. Ini sesuai
kontrak roadmap pada template.

---

## 5. Keadaan pengiriman

| Backend | Frontend | Integrasi | Verifikasi |
| --- | --- | --- | --- |
| `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` |

Perlu ditegaskan supaya tidak salah baca: `NOT_STARTED` di sini berarti **belum ada satu pun
task blueprint yang dikerjakan**, bukan berarti tidak ada source code. Backend HR sudah memuat
150 controller dan 1.343 endpoint, dan frontend sudah memuat 64 kelompok halaman master data.
Semua itu dibuat di luar alur blueprint, sehingga tidak dihitung sebagai pengiriman yang
tertelusur.

---

## 6. Bukti yang berpotensi basi

| Artefak | Diaudit pada | Diverifikasi masih berlaku pada | Keadaan |
| --- | --- | --- | --- |
| `01-existing-capability-map.md` | BE `ecdc135`, FE `2a1cea784` | BE `e0ee42c`, FE `fff76a1b39` | **`CURRENT`** |
| `00-interview-decisions.md` | BE `ecdc135`, FE `2a1cea784` | BE `e0ee42c`, FE `fff76a1b39` | **`CURRENT`** |

### 6.1 Baseline impact scan — 27 Agustus 2026

Baseline backend berpindah dari branch `AndryZain` ke **`origin/QuilvianIntegrationBackend`**
sebagai baseline canonical. Impact scan read-only dijalankan terbatas pada tujuh jalur.

| Field | Isi |
| --- | --- |
| SHA lama | `ecdc135444f0110482c9702212bcea30043983c8` — branch `AndryZain` |
| SHA baru | `16b8b71f4cd61e083213cf90722f4d768d339739` — `origin/QuilvianIntegrationBackend` |
| Hubungan | **Divergen**, bukan maju-mundur. Merge-base `7a0f60d2fc777c61b17dda79429eadf421046a2d` |
| Selisih commit | 9 commit hanya di Integration; 1 commit hanya di `ecdc135` |
| Frontend | `origin/AgentCodexFrontend` = `2a1cea784`, **sama persis** dengan yang tercatat. Tidak ada drift, impact scan frontend tidak diperlukan |

Hasil per jalur yang di-scan:

| Jalur | Berkas di `ecdc135` | Berkas di `16b8b71` | Selisih isi | Klasifikasi |
| --- | ---: | ---: | --- | --- |
| `Areas/Corporate/HumanResource/**` | 746 | 746 | Nihil | `NO_IMPACT` |
| `Areas/SelfServices/HumanResource/**` | 19 | 19 | Nihil | `NO_IMPACT` |
| `Shared/HumanResource/**` | 2 | 2 | Nihil | `NO_IMPACT` |
| `Repositories/Configurations/Corporate/HumanResource/**` | 354 | 354 | Nihil | `NO_IMPACT` |
| `Repositories/ApplicationDbContext.cs` | 1 | 1 | Nihil | `NO_IMPACT` |
| `Migrations/**` | 214 | 214 | Nihil | `NO_IMPACT` |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | 1 | 1 | Nihil | `NO_IMPACT` |

Seluruh tujuh jalur **identik byte per byte**. Tidak ada `CAPABILITY_IMPACT`,
`CONTRACT_IMPACT`, maupun `PERSISTENCE_IMPACT`.

Sembilan commit yang hanya ada di Integration seluruhnya pekerjaan Rawat Inap. Di luar
dokumentasi, hanya tiga berkas berubah dan tidak satu pun menyentuh HR:

| Berkas | Isi perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Controllers/BedController.cs` | Milik Health Services |
| `Areas/HealthServices/MasterData/DTOs/BedDtos.cs` | Milik Health Services |
| `QuilvianSystemBackend.csproj` | Menambahkan `QuilvianSystemBackend.Tests\**` pada `<Compile Remove>`. Tidak ada perubahan dependency, target framework, maupun SDK |

Satu commit yang hanya ada di `ecdc135` berisi **satu berkas**:
`docs/Modul-RS/PRD_to_MVP_HRD_Quilvian_Target_100.md`, 1.650 baris. Seluruh 746 berkas source HR
sudah ada sejak merge-base dan identik di kedua sisi.

### 6.2 Satu temuan yang perlu keputusan

| ID | Temuan | Klasifikasi | Dampak |
| --- | --- | --- | --- |
| `HRD-IMP-001` | Berkas `docs/Modul-RS/PRD_to_MVP_HRD_Quilvian_Target_100.md` **tidak ada** pada baseline canonical `QuilvianIntegrationBackend`. Berkas itu hanya hidup di branch `AndryZain` | `DECISION_IMPACT` — rendah | Blueprint merujuk berkas itu sebagai masukan produk pada `HRD-DEC-002`. Bila blueprint kelak masuk ke Integration, rujukan itu menunjuk berkas yang tidak ada di sana |

Dampaknya kecil karena `HRD-DEC-002` sudah menurunkan status berkas itu menjadi **masukan
produk**, bukan PRD yang berlaku, dan seluruh isi yang relevan sudah diserap ke
`00-interview-decisions.md` beserta tujuh koreksinya. Keputusan yang diperlukan: apakah berkas
itu ikut dibawa ke Integration, atau rujukannya diubah menjadi rujukan historis. Dicatat sebagai
`HRD-Q-16`.

### 6.3 Kesimpulan gate

**Capability map berstatus `CURRENT`.** Tidak ada capability yang perlu ditandai `STALE`, tidak
ada audit ulang, dan tidak ada fakta yang perlu diperbaiki. Bukti kemampuan yang dikumpulkan
pada `ecdc135` tetap berlaku penuh pada `16b8b71`.

Yang berubah hanya identitas baseline: pekerjaan berikutnya memakai
`origin/QuilvianIntegrationBackend` sebagai baseline canonical, bukan `AndryZain`, bukan
`master`, dan bukan `AgentCodexBackend`.

Satu catatan yang perlu diingat saat implementasi dimulai: karena kedua branch **divergen**,
memindahkan pekerjaan HR ke Integration bukan sekadar fast-forward. Penetapan branch kerja untuk
task HR berikutnya adalah keputusan pemegang modul, dan belum ada. Dicatat sebagai `HRD-Q-17`.

---

### 6.4 Drift scan — 30 Agustus 2026

Snapshot yang tercatat pada revision `4` sudah tertinggal dari HEAD kedua repository. Impact scan
read-only dijalankan untuk menentukan apakah ketertinggalan itu menyentuh kemampuan HR.

| Repository | SHA tercatat | HEAD aktual | Hubungan |
| --- | --- | --- | --- |
| Backend, branch `AndryZain` | `16b8b71` (baseline canonical) | `e0ee42c` | Maju |
| Frontend, branch `AgentCodexFrontend` | `2a1cea784` | `fff76a1b39` | Maju satu commit |

**Hasil backend.** Selisih `16b8b71..e0ee42c` menyentuh 113 berkas source di luar dokumentasi.
Tidak satu pun berada di jalur HR:

| Jalur yang di-scan | Selisih | Klasifikasi |
| --- | --- | --- |
| `Areas/Corporate/HumanResource/**` | **Nihil** | `NO_CAPABILITY_IMPACT` |
| `Areas/SelfServices/**` | **Nihil** | `NO_CAPABILITY_IMPACT` |
| `Shared/HumanResource/**` | **Nihil** | `NO_CAPABILITY_IMPACT` |
| `Repositories/Configurations/Corporate/HumanResource/**` | **Nihil** | `NO_CAPABILITY_IMPACT` |
| `Migrations/**` | **Nihil** | `NO_CAPABILITY_IMPACT` |

Seluruh perubahan source pada rentang itu milik Health Services — Emergency Installation
Management dan Clinical Management — ditambah pekerjaan dokumentasi Rawat Inap dan IGD.

**Hasil frontend.** Selisih `2a1cea784..fff76a1b39` berisi **tiga berkas**: `.claude/settings.json`,
`.gitignore`, dan `CLAUDE.md`. Seluruhnya governance, bukan source aplikasi.

| Klasifikasi | Isi |
| --- | --- |
| `NO_CAPABILITY_IMPACT` | Tidak ada route, view, component, hook, Redux slice, service, style, maupun asset yang berubah |

**Kesimpulan gate.** Capability map dan decision log tetap **`CURRENT`**. Tidak ada capability
yang perlu ditandai `STALE`, tidak ada audit ulang, dan tidak ada fakta yang perlu diperbaiki.
Snapshot SHA disegarkan sebagai pencatatan, bukan sebagai akibat perubahan kemampuan.

---

## 7. Langkah berikutnya yang disarankan

### 7.1 Apa yang sudah selesai

**Desain selesai sebagai `draft`.** Ketiga belas artefak canonical
`design-business-module` sudah ada seluruhnya.

| Artefak | Keadaan |
| --- | --- |
| `blueprint-manifest.md` | Ada, revision `5` |
| `00-interview-decisions.md` | Ada, revision `10` |
| `01-existing-capability-map.md` | Ada, revision `1.1` |
| `02-backend-architecture.md` | Ada, revision `1`, `draft` |
| `03-frontend-architecture.md` | Ada, revision `1`, `draft` |
| `04-prd-to-mvp.md` | **Baru pada revision `5`**, `draft` |
| `flowcharts/00-alur-utama.md` beserta 13 berkas proses | **Baru pada revision `5`**, `draft` |
| `data/data-dictionary.md` | **Baru pada revision `5`**, `draft` |
| `contracts/` lima berkas | Ada, `v1`, `draft` |
| `testing/acceptance-test-matrix.md` | **Baru pada revision `5`**, `draft` |

Folder `erd/` **tidak ada dan tidak boleh dibuat**. Kontrak keluaran terbaru menghapusnya sebagai
artefak; penggantinya tercantum pada `blueprint-manifest.md` bagian 7.

### 7.1a Keputusan pemilik yang ditutup 30 Agustus 2026

| ID | Menutup | Status |
| --- | --- | --- |
| `HRD-DEC-031` | `HRD-Q-19` | `approved` |
| `HRD-DEC-032` | `HRD-Q-33` | **`SECURITY_APPROVED`** |
| `HRD-DEC-033` | `HRD-Q-20` | **`SECURITY_APPROVED`** |
| `HRD-DEC-034` | Isi konfigurasi workflow | `approved` untuk prinsip; isi rantai menunggu tinjauan |
| `HRD-DEC-035` | `HRD-Q-49` untuk cakupan MVP | `approved` |
| `HRD-DEC-036` | `HRD-Q-54` | `approved` — empat definisi alur terpisah |
| `HRD-DEC-037` | Kewenangan konfigurasi kebijakan gaji | `approved` — kontrak sasaran |
| `HRD-DEC-038` | Kepemilikan slip gaji dan otentikasi bertingkat | `approved` — kontrak sasaran |
| `HRD-DEC-039` | Audit pembacaan gaji sensitif | `approved` — kontrak sasaran |
| `HRD-DEC-040` | Perlindungan HTTP dan sisi klien | `approved` — kontrak sasaran |
| `HRD-DEC-041` | Jenjang Pendidikan sebagai dimensi kebijakan gaji | `approved` — kontrak sasaran |
| ~~`HRD-DEC-042`~~ | ~~Masa Kerja sebagai dimensi tersendiri~~ | **`SUPERSEDED FOR CURRENT MVP`** oleh `HRD-DEC-045`. Dipertahankan sebagai sejarah |
| `HRD-DEC-043` | Kebijakan gaji berversi dan dapat dikonfigurasi | `approved` — kontrak sasaran |
| `HRD-DEC-044` | Payroll Officer tanpa `: ViewAmount` | `approved` |
| `HRD-DEC-045` | **Masa kerja bukan faktor kebijakan gaji MVP saat ini**; menggantikan `HRD-DEC-042` | `approved` |

`HRD-Q-55` **ditutup**.

`HRD-Q-56` berstatus **`DEFERRED / NOT_APPLICABLE_TO_CURRENT_MVP`** sejak `HRD-DEC-045`
mengeluarkan masa kerja dari faktor kebijakan gaji. Ia **tidak perlu dijawab** untuk MVP
administratif.

**Co-sign keamanan selesai 2026-08-30** oleh `Project final decision authority — Security`.
Hasilnya tercatat pada [`evidence/02-security-review-packet.md`](./evidence/02-security-review-packet.md).

`HRD-Q-55` **ditutup** `HRD-DEC-041` dan `HRD-DEC-042` pada revisi `9`.

**Isi rantai `T1` s.d. `T8` sudah disetujui** pada decision log bagian 27.2. `HRD-DEC-034` tidak
lagi berstatus usulan. **Master data workflow belum diisi** — menyetujui isi konfigurasi tidak
sama dengan mengisi datanya; pengisian dijadwalkan pada `MVP-0`.

`HRD-Q-54` **ditutup** `HRD-DEC-036`: empat definisi alur terpisah dengan pola persetujuan awal
yang sama.

### 7.2 Langkah berikutnya

**Yang diperlukan sekarang adalah approval manusia, bukan skill berikutnya.**

| Urutan | Tindakan | Pemilik |
| --- | --- | --- |
| 1 | Tinjau `04-prd-to-mvp.md` — batas MVP, gelombang pengiriman, dan Definition of Done | Pemilik produk HR bersama technical owner |
| 2 | ~~Tinjau paket keamanan~~ — **selesai 2026-08-30**, kedua keputusan `SECURITY_APPROVED` | ~~Pemilik keamanan~~ |
| 3 | Setujui `02-backend-architecture.md`, `03-frontend-architecture.md`, dan kelima kontrak | Owner masing-masing |
| 4 | Baru setelah itu, `plan-module-delivery` | — |

**`plan-module-delivery` MUST NOT dijalankan sekarang.** Seluruh kontrak masih `draft`, dan
`04-prd-to-mvp.md` memuat pertanyaan memblokir yang belum terjawab. Kontrak keluaran menyatakan
dokumen dengan pertanyaan memblokir **MUST NOT** diteruskan ke perencanaan delivery.

### 7.3 Pertanyaan yang memblokir gelombang pengiriman

| Pertanyaan | Pemilik | Yang terblokir |
| --- | --- | --- |
| `HRD-Q-47` — dampak izin pulang cepat terhadap saldo dan pembayaran | Pemilik proses HR | `EPIC HRD-03` bagian izin pulang cepat |
| `HRD-Q-10` — bentuk data yang diterima Finance | Pemilik produk bersama Finance | **`POST-MVP` saja.** Tidak memblokir MVP administratif sejak `HRD-DEC-035` |
| `HRD-Q-11` — perilaku bila Finance menolak batch | Pemilik produk bersama Finance | **`POST-MVP` saja.** Sama |
| `HRD-Q-05` — isi tabel 67 entity yang belum punya API | Pemilik basis data | Seluruh `POST-MVP` domain tanpa API; pemasangan unique constraint pada penempatan shift |
| `HRD-Q-08` — wakil Komite Medik | Manajemen | `S-C1` |
| Pengesahan `HRD-DEC-010` | K3RS | `S-C6` |
| `HRD-Q-51` — pemisahan peran pada tindakan disiplin | Pemilik proses HR | `S-C5` |
| `HRD-Q-52` — tingkatan izin data paling terbatas | Pemilik keamanan bersama pemilik proses | `S-C5` |
| `HRD-Q-01` — pemilik kebijakan bisnis HR | Manajemen | Approval blueprint secara keseluruhan |

---

## 8. Kontrak status

Status modul memakai nilai dari template: `DRAFT` sudah punya identitas tetapi asupannya belum
lengkap; `DISCOVERY` sedang mengumpulkan keputusan dan bukti; `READY` berarti fase yang
direncanakan boleh mulai; `PARTIAL` berarti setidaknya satu fase siap sementara fase lain
terblokir atau belum diketahui; `BLOCKED` berarti tidak ada fase material yang aman dijalankan;
`IN_PROGRESS` punya pekerjaan aktif yang sudah diberi wewenang; `VERIFYING` menunggu bukti
kesiapan; `DONE` menuntut bukti verifikasi yang sesuai; `SUPERSEDED` mencatat blueprint
penggantinya.

Status fase memakai `NOT_STARTED`, `READY`, `IN_PROGRESS`, `BLOCKED`, `DONE`, dan `SUPERSEDED`.
Sebuah fase menjadi `DONE` hanya ketika bukti acceptance atau kesiapannya tercatat; keberadaan
berkas saja tidak cukup.

Klasifikasi kesiapan desain pada bagian 1 — `READY FOR DESIGN`, `PARTIAL`, `BLOCKED`,
`DEFERRED` — adalah lapisan terpisah yang menjawab pertanyaan berbeda, yaitu boleh tidaknya
sesuatu dirancang. Keduanya berdampingan dan tidak saling menggantikan.
