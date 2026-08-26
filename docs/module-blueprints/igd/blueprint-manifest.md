# IGD Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `IGD-BP-001` |
| `revision` | `5` |
| `status` | `draft` — **tidak disetujui siapa pun**. Revision `4` yang berstatus `approved sebagian` tetap berlaku sebagai baseline sampai revisi ini disetujui |
| `module` | `igd` / `EmergencyInstallationManagement`, prefix entity `TrxEmergency`/`MstEmergency` |
| `registry_lifecycle` | `ACTIVE` |
| `design_snapshot_at` | `2026-08-24` |
| `backend_commit_sha` | `f69e9e483052845d11c91d8b7bbdce33c4acc8d8` (branch `rizkiG`) |
| `frontend_commit_sha` | `96a9120111f6acc6b7c0f37973ea0c717ba41f17` (branch `RizkiV2`) |
| `owners` | Product/Domain: **Rizki Gunawan**, ditetapkan `IGD-DEC-089` 2026-08-24. Clinical Governance, Nursing authority, Security/Privacy: `OPEN`. Pemilik `ClinicalManagement`, `PharmacyManagement`, `LaboratoryManagement`: **belum ditunjuk** |
| `approved_by` | Sebagian: **Rizki Gunawan** menyetujui `IGD-DEC-067`, `IGD-DEC-088`, `IGD-DEC-089`, dan `IGD-DEC-093` pada 24 Agustus 2026. Blueprint secara keseluruhan **belum** disetujui |
| `approved_at` | Sebagian: `2026-08-24` |
| `requirement_readiness` | **`UNCLASSIFIED`** — lihat bagian 0 |
| `domain_architecture_revision` | **Tidak ada** — lihat bagian 0 |
| `domain_architecture_readiness` | **`NOT_ASSESSED`** |
| `input_revisions` | `00-interview-decisions.md` 91 keputusan (`IGD-DEC-090`…`092` ditambahkan Amendment Pass kedua 2026-08-24); `01-existing-capability-map.md` revision `3` |
| `amendment` | **2026-08-24 (kedua)** — `IGD-OQ-068`, `IGD-OQ-070`, `IGD-OQ-071` ditutup. Desain revision `5` **tidak berubah isinya**: ketiga jawaban membenarkan bentuk teknis yang sudah tertulis. Yang berubah hanya status pertanyaan dan gerbang |
| `contract_versions` | API `0.3.0`; state `0.3.0`; validation `0.3.0`; integration `0.3.0`; permission/audit `0.3.0`. Seluruhnya `draft` **kecuali** dua irisan yang di-`approved` `IGD-DEC-093` 2026-08-24: state bagian 1/1.1/1.2, dan validation bagian 2 aturan 4–5 |
| `roadmap_revision` | `2` — gelombang `MVP-0`, terbit 2026-08-24. Revision `1` diarsipkan ke `roadmap/archive/revision-1/` |
| `compatibility_impact` | **Empat perubahan memutus.** Lihat bagian 3 |

---

## 0. Gerbang kemampuan rumah sakit — BELUM TERPENUHI

Modul IGD **tidak memiliki** dua artefak hulu yang diwajibkan untuk kemampuan bisnis rumah
sakit:

| Artefak | IGD | Rawat Inap sebagai pembanding |
|---|---|---|
| `evidence/02-requirement-completeness-gate.md` | **Tidak ada** | Ada |
| `evidence/03-hospital-domain-architecture.md` | **Tidak ada** | Ada |

Akibatnya modul IGD tidak punya klasifikasi kesiapan requirement per slice. Substansinya
tersebar pada 88 keputusan, bukan pada berkas hulu tersendiri.

Gerbang ini **tidak** ditandai terpenuhi. Bila Product/Domain Owner menghendaki kesetaraan
dengan Rawat Inap, `/requirement-completeness-gate` dan `hospital-domain-architect` perlu
dijalankan lebih dulu dan revisi ini ditinjau ulang terhadap hasilnya.

---

## 1. Yang berubah pada revision 5

| Area | Perubahan |
|---|---|
| Capability map | Revision `2` **dibuang seluruhnya**, diganti revision `3`. Bukti lama menunjuk nama repository dan path yang tidak ada lagi |
| Keputusan | Dua puluh dua keputusan baru `IGD-DEC-067` sampai `IGD-DEC-088`; `IGD-DEC-081` `superseded` sebagian |
| Jenis kunjungan | Kunjungan IGD menjadi `EncounterType.Emergency` |
| Pencatatan klinis | Pembatas antrean dan konsultasi dilonggarkan untuk kunjungan `Emergency` |
| Kepergian pasien | `TrxEmergencyTransfer` menjadi `TrxEmergencyDeparture` dengan dua rangkaian status |
| Tempat tidur | Seluruh urusan tempat tidur pindah ke Rawat Inap |
| Kepemilikan pasien | Matriks lengkap tanpa satu pun keadaan tanpa pemilik |
| Audit klinis | Koreksi bersifat tambah-saja |
| Kewenangan unit | Jembatan `MstServiceUnit` ke simpul organisasi, bukan tabel penugasan baru |
| Dokumen baru | `04-prd-to-mvp.md`, `erd/emergency-departure.md` |
| Arsip | Revision `4` disimpan di `archive/`, tidak dihapus |

---

## 2. Artifact hashes

Dihitung 24 Agustus 2026 pada keadaan akhir seluruh berkas.

| Artifact | SHA-256 |
|---|---|
| `00-interview-decisions.md` | `43ba0661bf30d0bd626bca8d4592abbfb6a334fe18dffeaba2d9d4ad1bbb7fb0` — dihitung ulang 2026-08-24 setelah Amendment Pass kedua **dan** approval sempit `IGD-DEC-093`. Hash pada revision `5` semula `d3188d29e5e872e99ba4fc030af18a9b9a4a519bff7ea4404a560ca2261f0ad8` |
| `01-existing-capability-map.md` | `d8e092d3d9d71b0679c6a690979de488b2c51d4cdf32864915bc1c7fe544ec67` |
| `02-backend-architecture.md` | `20fcaad625ab52b7058f751cad96c8732d234264d1d94a28b1f1ccd6f3aa6753` |
| `03-frontend-architecture.md` | `2b4339f9587ed1daff8444ccb68cb5415df578d76a2157dd3ec168f9a2a1fd95` |
| `04-prd-to-mvp.md` | `7061525001d9a7e6b311424b8e3a8d85de13e35f59e545a78dcefedd600b79db` |
| `erd/00-context-erd.md` | `60c862c6516e6bc641c3fea61725d58fa805a2acfdab5315c567f250a3403c63` |
| `erd/emergency-episode.md` | `43f2403785d3b0cd5ba6bdd59854b8f24eb0904e03d8f8d746a0772a0a727822` |
| `erd/emergency-departure.md` | `94f8362fa7b8cad023fb539cdb1de61d21a629c2ae0a83a03395bf258b172251` |
| `erd/data-dictionary.md` | `fb6586a130ba527c3d27ec979c66f5a3e3f8a212270f928a16440170f28b0636` |
| `contracts/api-contract.md` | `1efcac528e2360fdeccd9b7373d724727a3f4d51f5acf34f496e14e3a62e6d62` |
| `contracts/state-transition-matrix.md` | `a41efd8d9adc87e1cf1eec2a9397b3521fdc0ebf935ccf0a19a5aa975b6c7c75` — dihitung ulang setelah `IGD-DEC-093` menandai bagian 1/1.1/1.2 `approved`. Hash semula `770cf8ea0517ebdcdc71be6409cf3c7071e83663f57446488bd52196e7cb6faa` |
| `contracts/validation-matrix.md` | `0ee98b750a29e01603db894ed3766614fe8989b2eef3573eab7d72cdc1a6b907` — dihitung ulang setelah `IGD-DEC-093` menandai bagian 2 aturan 4–5 `approved`. Hash semula `5ab17e6b540071adcbe797c074193edc74c8648e05cd94d19291559050d1abfc` |
| `contracts/integration-contract.md` | `0b3ea15dd192023f6798cc3c4caa10ec96355c0598357783eae6bf8c2eafe5a5` |
| `contracts/permission-audit-matrix.md` | `dae15a4c1c85712659379928f2da5a233d8c2de89c73533862808b90e646bc5b` |
| `testing/acceptance-test-matrix.md` | `0795daa024928a583b3b7ca4ef75e15abedac5f7c937814c14dec6a3ad392b8e` |

Manifest tidak menghitung hash dirinya sendiri.

`roadmap/` **tidak** diperbarui pada revisi ini. Isinya masih roadmap revision `1` yang
seluruh task-nya sudah dikerjakan. Roadmap baru adalah keluaran `/qv-plan`, bukan `/qv-design`.

---

## 3. Dampak kompatibilitas

### 3.1 Empat perubahan yang memutus

| Perubahan | Siapa yang terdampak |
|---|---|
| Kunjungan IGD wajib `EncounterType.Emergency` | Setiap pemanggil yang mengirim `Outpatient`, termasuk test `FE-IGD-001 K1` yang **akan gagal** |
| Grup `emergency-transfers` menjadi `emergency-departures` | Seluruh pemanggil route lama |
| `TransferStatus` dipecah menjadi dua kolom | Pembaca satu kolom status |
| Empat field tempat tidur dan ruangan dihapus | Pengirim field tersebut |

### 3.2 Perubahan pada tabel milik modul lain

**Sembilan tabel** yang bukan milik IGD terdampak: `TrxPatientEncounter`, `MstServiceUnit`,
`TrxPatientAssessment`, `TrxDoctorConsultation`, `TrxPatientDiagnosis`, `TrxPatientProcedure`,
`TrxPatientVitalSign`, `TrxPatientIntegratedProgressNote`, `TrxPrescription`.

Janji "nol perubahan kolom pada tabel modul lain" yang dipegang blueprint Rawat Inap **tidak
dapat dipertahankan** untuk IGD, dan `IGD-DEC-075` juga membatalkannya bagi Rawat Inap.

### 3.3 Yang bersifat aditif

Empat tabel baru, sebelas endpoint baru, lima enum baru, tiga service baru, dua controller
baru. Tidak satu pun memutus pemakai lama.

---

## 4. Gerbang sebelum produksi

| Gerbang | Menunggu | Memblokir |
|---|---|---|
| Penunjukan pemilik `ClinicalManagement` dan `PharmacyManagement` | Organisasi | **`EPIC IGD-09`** — pengkajian, diagnosis, tindakan, resep IGD |
| Persetujuan Muhammad Hamzah atas revisi `RWI-RULE-026` dan `compatibility_impact` | Product/Domain Owner Rawat Inap | `EPIC IGD-09` |
| Persetujuan pemilik IGD atas `RWI-OQ-034` / `DEC-INP-002` | **Pemilik IGD, nama belum tercatat** | Slice `INP-S09` milik Rawat Inap |
| ~~`IGD-OQ-068` penafsiran kolom status dan tabel kejadian~~ | **Terjawab** `IGD-DEC-090` 2026-08-24 | — |
| ~~`IGD-OQ-070` penggantian nama tabel dan route~~ | **Terjawab** `IGD-DEC-091` 2026-08-24 | — |
| ~~`IGD-OQ-071` perilaku unit tanpa pemetaan organisasi~~ | **Terjawab sementara** `IGD-DEC-092` 2026-08-24. Pengesahan Security/Privacy owner masih ditunggu | Penyalaan penjagaan di produksi, **bukan** implementasi |
| Pengisian pemetaan unit selesai **sebelum** penjagaan dinyalakan | Pemilik Master Data | Syarat melekat `IGD-DEC-092`; `MVP-5` |
| `IGD-UNK-01` … `IGD-UNK-07` | Kueri ke basis data bersama | `MVP-1`, `MVP-3`, `MVP-5` |
| Otorisasi menjalankan migration | Pemilik basis data pengembangan | Seluruh gelombang |
| Data master kelas pasien IGD | Penanggung jawab data master | `MVP-1` |
| Pemetaan unit ke simpul organisasi | Master Data + Corporate/HR | `MVP-5` |
| SOP triase dan SOP pengkajian ulang | Clinical governance MMC | Nilai batas waktu; **tidak** memblokir kode |
| Break-glass dan pemisahan SuperAdmin | Security/Privacy owner | Produksi |

Gerbang yang belum terpenuhi berarti menolak tindakan privileged, integrasi, atau finansial
yang terdampak. Gerbang **tidak pernah** memblokir pelayanan klinis darurat.

### 4.0 Amendment 2026-08-24 (kedua) — larangan `/qv-plan` dicabut

`04-prd-to-mvp.md` bagian 7 berbunyi: dokumen ini *"**tidak boleh** diteruskan ke `/qv-plan`
sebelum `IGD-OQ-068`, `IGD-OQ-070`, dan `IGD-OQ-071` dijawab"*. **Ketiganya sudah dijawab**
pada 24 Agustus 2026 lewat `IGD-DEC-090`, `IGD-DEC-091`, dan `IGD-DEC-092`. Larangan itu
**tidak lagi berlaku**, dan `/qv-plan` boleh berjalan untuk seluruh gelombang.

`04-prd-to-mvp.md` **sengaja tidak disunting** oleh Amendment Pass: menyunting keluaran
`/qv-design` dari dalam pass wawancara melanggar batas peran, dan hash-nya akan melenceng
tanpa perhitungan ulang seluruh artefak. Manifest inilah yang berwenang atas keadaan gerbang.
Bagian 7 dokumen tersebut akan diselaraskan pada pass `/qv-design` berikutnya.

`IGD-DEC-092` bersifat **sementara** — ia membuka implementasi `EPIC IGD-08`, tetapi
**tidak** membuka penyalaan penjagaan di produksi. Dua hal itu berbeda.

### 4.1 Satu gelombang yang tidak terhalang apa pun

**`MVP-0` tidak bergantung pada satu pun gerbang di atas.** Isinya: pengisian master kelas
pasien IGD, pemetaan unit ke simpul organisasi, dan `EPIC IGD-03` — perbaikan status kunjungan
yang dapat mundur. `EPIC IGD-03` adalah perbaikan cacat murni yang tidak memerlukan keputusan
siapa pun.

---

## 5. Yang tidak dikerjakan revisi ini

| Yang tidak dikerjakan | Alasan |
|---|---|
| Menandai desain `approved` | Approval adalah tindakan manusia |
| Source code, migration, endpoint, atau UI | Di luar wewenang tahap desain |
| Roadmap dan task | Keluaran `/qv-plan` |
| Pemetaan proses ke ClickUp | Tidak diminta |
| Menjalankan kueri ke basis data | Basis data dipakai bersama satu tim |
| Memperbarui `blueprint-manifest.md` milik Rawat Inap | Bukan wewenang IGD; diusulkan, bukan diberlakukan |

---

## 6. Impact scan 2026-08-24

| Repository | SHA capability map revision 3 | SHA saat desain disusun | Hasil |
|---|---|---|---|
| Backend | `f69e9e48` | `f69e9e48` | **Sama.** Nol berkas `.cs` berubah |
| Frontend | `96a91201` | `96a91201` | **Sama.** Working tree bersih |

Bukti source pada capability map revision `3` karena itu **sahih** dan tidak perlu diaudit
ulang sebelum implementasi dimulai.
