# Requirement Traceability — Modul Rawat Inap

## Metadata

```yaml
module_id: rawat-inap
roadmap_revision: 2
status: APPROVED
approval_gate: BLUEPRINT_APPROVED
approved_by:
  - "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061), lewat RWI-DEC-067; sinkronisasi revision 2 lewat RWI-DEC-074"
approved_at: "2026-08-24"
input_revisions:
  blueprint-manifest.md: 4
  00-interview-decisions.md: 6
  04-prd-to-mvp.md: 0.4.0
  testing/acceptance-test-matrix.md: 0.4.0
contract_versions:
  - "API 0.4.0"
  - "State transition 0.4.0"
  - "Validation 0.4.0"
  - "Integration 0.4.0"
  - "Permission/Audit 0.4.0"
  - "Acceptance test 0.4.0"
  - "PRD ke MVP 0.4.0"
counts:
  epic: 14
  functional_requirement: 62
  acceptance_criteria: 139
  uat_scenario: 33
  backend_task: 33
  frontend_task: 19
  api_endpoint_baru: 49
  api_endpoint_perubahan_perilaku: 1
```

---

## 0. Cara memakai dokumen ini

Dokumen ini menjawab satu pertanyaan: **apakah ada requirement yang tidak dikerjakan siapa pun, dan
apakah ada task yang tidak menjawab requirement apa pun?** Keduanya sama berbahayanya — yang pertama
adalah lubang cakupan, yang kedua adalah pekerjaan yang tidak ada yang memintanya.

Arah bacanya dua:

```text
EPIC → FR → task → acceptance criteria → skenario test        (bagian 1 dan 2)
task → EPIC                                                    (bagian 3, arah balik)
```

Baris yang berbunyi "menyusul" **tidak diperbolehkan** di dokumen ini. Bila sesuatu belum dapat
ditelusuri, tulis alasannya dan Decision ID yang menahannya.

---

## 1. Epic → task

| Epic | Isinya | Gelombang | Task backend | Task frontend |
| --- | --- | --- | --- | --- |
| `EPIC RI-21` | Fondasi episode dan data master | `MVP-0` | `BE-RWI-001`, `BE-RWI-002`, `BE-RWI-003`, `BE-RWI-004`, `BE-RWI-007`, `BE-RWI-008` | `FE-RWI-002`, `FE-RWI-006` |
| `EPIC RI-22` | Pencarian dan pemesanan tempat tidur | `MVP-1` | `BE-RWI-010` | `FE-RWI-005` |
| `EPIC RI-23` | Penempatan pasien dan pengaktifan episode | `MVP-1` | `BE-RWI-011`, `BE-RWI-012` | `FE-RWI-007` |
| `EPIC RI-24` | Census dan lama dirawat | `MVP-1` | `BE-RWI-016` | `FE-RWI-008` |
| `EPIC RI-25` | Penanggung jawab episode | `MVP-2` | `BE-RWI-017`, `BE-RWI-018` | `FE-RWI-011` |
| `EPIC RI-26` | Perpindahan pasien dan pindah kelas | `MVP-2` | `BE-RWI-019` | `FE-RWI-010` |
| `EPIC RI-27` | Keputusan pulang dan resume | `MVP-3` | `BE-RWI-020`, `BE-RWI-021`, `BE-RWI-022` | `FE-RWI-012` |
| `EPIC RI-28` | Daftar periksa, kelayakan keuangan, dan penutupan | `MVP-3` | `BE-RWI-023`, `BE-RWI-024`, `BE-RWI-025`, `BE-RWI-026`, `BE-RWI-027` | `FE-RWI-013`, `FE-RWI-014`, `FE-RWI-015` |
| `EPIC RI-29` | Riwayat status dan daftar pantau | `MVP-4` | `BE-RWI-028`, `BE-RWI-029` | `FE-RWI-016`, `FE-RWI-017` |
| `EPIC RI-30` | Sesi koreksi episode | `MVP-4` | `BE-RWI-030` | `FE-RWI-018` |
| `EPIC RI-31` | Pengaturan yang dapat diubah admin | `MVP-0` | `BE-RWI-005` | `FE-RWI-003`, `FE-RWI-004` |
| `EPIC RI-32` | Perbaikan tempat tidur dan pembatasan wewenang status | `MVP-0` | `BE-RWI-006`, `BE-RWI-032` | `FE-RWI-001` |
| `EPIC RI-33` | Bayi baru lahir dan boks bayi | `MVP-4` | `BE-RWI-031` | Tidak ada layar khusus — tercakup census dan penempatan |
| `EPIC RI-34` | Kelayakan penempatan menurut jenis kelamin dan isolasi | `MVP-1` | `BE-RWI-013`, `BE-RWI-014`, `BE-RWI-015` | `FE-RWI-006`, `FE-RWI-007`, `FE-RWI-009`, `FE-RWI-016` |

**Empat belas epic, nol tanpa task.**

Dua task tidak menempel pada epic mana pun, dan itu disengaja: `BE-RWI-033` bukti penerimaan dan
`FE-RWI-019` kesiapan e2e. Keduanya menjawab `NFR-008` dan `RWI-DEC-051`, bukan satu epic tertentu.

---

## 1b. Progres delivery per 24 Agustus 2026

Bagian ini memisahkan dua hal yang sering dicampur: **kode yang selesai**, versus **DoD yang
benar-benar terpenuhi** menurut aturan roadmap sendiri.

| Task | Requirement yang dijawab | Bukti | Status |
| --- | --- | --- | :---: |
| `BE-RWI-001` | Angka batas waktu dan butir administrasi punya tempat tinggal di master, bukan di kode (`RWI-DEC-008`, `RWI-DEC-026`, `RWI-DEC-032`) | Build Release lulus; `has-pending-model-changes` bersih; migration **maju dan mundur lulus** pada PostgreSQL 16 lokal sekali pakai; bentuk kolom cocok kolom demi kolom dengan `erd/data-dictionary.md` bagian 12 dan 13; unique `ItemCode` terbukti menolak duplikat di database sungguhan ([laporan](../../../task/report/be-rwi-001-tabel-master-rawat-inap.md)) | ✅ **Selesai** |
| `BE-RWI-002` s.d. `BE-RWI-033` | — | Belum dikerjakan | Planned |

**1 dari 33 task backend selesai (3%).** Tidak ada task yang berstatus selesai-sebagian.

### Yang perlu diputuskan pemilik pekerjaan

| Butir | Sifat | Terdampak |
| --- | --- | --- |
| Tidak ada connection string lokal — `dotnet ef database update` polos mengenai database dev bersama `QuilvianNewDevTim01` | Operasional, dapat dikerjakan | Setiap task bermigration berikutnya, terutama `BE-RWI-003` |
| Ketiga berkas migration `BE-RWI-001` belum di-commit | Operasional | Rekan yang menarik branch `MHamzah` |
| Letak konfigurasi EF master: `HealthServices/MasterData/` versus `HealthServices/` | Konvensi, tanpa akibat teknis | Konsistensi folder, sejalan `BE-IGD-013` |
| `RWI-RULE-021` belum final secara klinis — nilai `24` jam terpasang sebagai bawaan | Klinis | Menahan pemakaian untuk pasien sungguhan, bukan MVP |

---

## 2. Functional requirement → task → acceptance criteria → test

Kolom **AC** merujuk `00-interview-decisions.md` revision `5`. Kolom **Test** merujuk
`testing/acceptance-test-matrix.md` `0.3.0` dan skenario `UAT` pada `04-prd-to-mvp.md`.

### `EPIC RI-21` — Fondasi episode dan data master

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-101` s.d. `FR-RI-104` | Episode, nomor, jangkar kunjungan, DPJP wajib | `BE-RWI-007` | `RWI-AC-004` s.d. `RWI-AC-006` | Bagian 1; `UAT-01` |
| `FR-RI-105` s.d. `FR-RI-108` | Ubah isian, pembatalan, kedaluwarsa `Draft` | `BE-RWI-008` | `RWI-AC-007` s.d. `RWI-AC-010`, `RWI-AC-090` s.d. `RWI-AC-092` | Bagian 6; `UAT-23` |
| `FR-RI-148` | Satu pasien satu episode yang hadir | `BE-RWI-012` | `RWI-AC-116`, `RWI-AC-117` | Bagian 1; `UAT-26` |

### `EPIC RI-22` — Pencarian dan pemesanan tempat tidur

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-106` s.d. `FR-RI-108` | Pemesanan 2 jam, gugur saat dibaca, dapat diubah admin | `BE-RWI-010` | `RWI-AC-001` s.d. `RWI-AC-003` | Bagian 1; `UAT-03` |

### `EPIC RI-23` — Penempatan pasien

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-109` | Pencegahan tempat tidur ganda | `BE-RWI-011` | `RWI-AC-059` | Bagian 2 skenario tabrakan; `UAT-02`, `UAT-04` |
| `FR-RI-110` | Penempatan dan salinan status dalam satu transaksi | `BE-RWI-011` | `RWI-AC-062` | Bagian 2 |
| `FR-RI-111` | Pemesanan gugur tidak menghalangi penempatan | `BE-RWI-011` | `RWI-AC-002` | Bagian 1 |
| `FR-RI-112` | Penolakan tidak menghapus isian admisi | `BE-RWI-011`, `FE-RWI-007` | `RWI-AC-010` | Bagian 2 |
| `FR-RI-148` | Satu pasien satu episode | `BE-RWI-012` | `RWI-AC-116`, `RWI-AC-117` | Bagian 1; `UAT-26` |

### `EPIC RI-24` — Census dan lama dirawat

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-113` | Census hanya `Admitted` dan `DischargePending` | `BE-RWI-016` | — | Bagian 5; `UAT-06` |
| `FR-RI-114` | Lama dirawat dari selisih tanggal, minimum 1 hari | `BE-RWI-016` | — | Bagian 5 unit test; `UAT-05` |
| `FR-RI-115` | Bertambah pada pergantian tanggal | `BE-RWI-016` | — | Bagian 5 unit test |

### `EPIC RI-25` — Penanggung jawab episode

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-116` | DPJP berbentuk riwayat berperiode | `BE-RWI-017` | — | Bagian 4; `UAT-07` |
| `FR-RI-117` | Tepat satu DPJP aktif | `BE-RWI-017` | — | Bagian 4 |
| `FR-RI-118` | Pengalihan wajib beralasan | `BE-RWI-017`, `FE-RWI-011` | — | Bagian 4 |
| `FR-RI-119` | Episode boleh tanpa perawat | `BE-RWI-018`, `FE-RWI-011` | — | Bagian 4 |

### `EPIC RI-26` — Perpindahan pasien

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-120` | Perpindahan bersifat utuh | `BE-RWI-019` | — | Bagian 3; `UAT-09` |
| `FR-RI-121` | Kelas mengikuti kamar | `BE-RWI-019` | — | Bagian 3 |
| `FR-RI-122` | Dokter bukan DPJP tidak dapat memindahkan | `BE-RWI-019`, `FE-RWI-010` | — | Bagian 3; `UAT-08` |
| `FR-RI-123` | Perpindahan wajib beralasan medis | `BE-RWI-019` | — | Bagian 3 |
| `FR-RI-162` | Aturan penempatan berlaku pada perpindahan | `BE-RWI-019` | `RWI-AC-133` | Bagian 2A.1 |

### `EPIC RI-27` — Keputusan pulang dan resume

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-124` s.d. `FR-RI-128` | Keputusan pulang, lima cara pulang, resume, tanda tangan | `BE-RWI-020`, `BE-RWI-021` | — | Bagian 7; `UAT-10` |
| `FR-RI-153` | Versi resume pulang | `BE-RWI-022` | `RWI-AC-124` s.d. `RWI-AC-126` | Bagian 7; `UAT-27` |

### `EPIC RI-28` — Daftar periksa, kelayakan keuangan, dan penutupan

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-129` s.d. `FR-RI-133` | Daftar periksa, kelayakan keuangan, lima syarat, penutupan | `BE-RWI-023`, `BE-RWI-024`, `BE-RWI-025` | `RWI-AC-064` | Bagian 8, 9; `UAT-11` |
| `FR-RI-134` | Jalan keluar supervisor | `BE-RWI-026`, `FE-RWI-014` | — | Bagian 8; `UAT-12`, `UAT-13` |
| `FR-RI-149` s.d. `FR-RI-151` | Kepergian fisik pasien | `BE-RWI-027`, `FE-RWI-015` | `RWI-AC-118` s.d. `RWI-AC-121` | Bagian 4A; `UAT-24`, `UAT-25` |

### `EPIC RI-29` — Riwayat status dan daftar pantau

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-135` s.d. `FR-RI-138` | Riwayat status, tiga daftar pantau, laporan selisih | `BE-RWI-028`, `BE-RWI-029` | `RWI-AC-063` | Bagian 10; `UAT-17`, `UAT-21` |

### `EPIC RI-30` — Sesi koreksi

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-139` s.d. `FR-RI-141` | Sesi koreksi supervisor, tidak mengganggu tempat tidur | `BE-RWI-030`, `FE-RWI-018` | — | Bagian 10; `UAT-14`, `UAT-15`, `UAT-16` |

### `EPIC RI-31` — Pengaturan admin

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-142` s.d. `FR-RI-144` | Pengaturan dan butir administrasi dapat diubah admin | `BE-RWI-005`, `FE-RWI-003`, `FE-RWI-004` | `RWI-AC-003`, `RWI-AC-105` s.d. `RWI-AC-107` | Bagian 11; `UAT-18`, `UAT-19` |

### `EPIC RI-32` — Perbaikan tempat tidur dan pembatasan wewenang status

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-145` | Status terisi dan dipesan hanya dari Rawat Inap | `BE-RWI-006` | `RWI-AC-060`, `RWI-AC-061` | Bagian 12; `UAT-20`, `UAT-21` |
| — | Tombol tempat tidur tidak lagi 404 | `FE-RWI-001` | `RWI-AC-114` | Bagian 12 |
| — | Modul tetangga terbukti tidak rusak | `BE-RWI-032` | `RWI-AC-114` | Bagian 12 |

### `EPIC RI-33` — Bayi baru lahir dan boks bayi

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-146` | Boks bayi sebagai tempat tidur | `BE-RWI-031` | — | Bagian 12A; `UAT-22` |
| `FR-RI-147` | Episode ibu dan bayi terpisah | `BE-RWI-031` | `RWI-AC-123` | Bagian 12A |
| `FR-RI-152` | Penanda rawat gabung | `BE-RWI-031` | `RWI-AC-122` | Bagian 12A; `UAT-28` |

### `EPIC RI-34` — Kelayakan penempatan menurut jenis kelamin dan isolasi

| FR | Isinya | Task | AC | Test |
| --- | --- | --- | --- | --- |
| `FR-RI-154` | Penanda tempat tidur menolak jenis kelamin | `BE-RWI-013` | `RWI-AC-128` | Bagian 2A.1 |
| `FR-RI-155` | Kamar tidak boleh campur | `BE-RWI-013`, `FE-RWI-007` | `RWI-AC-130` | Bagian 2A.1; `UAT-29` |
| `FR-RI-156` | Boks bayi dikecualikan dua arah | `BE-RWI-013` | `RWI-AC-131`, `RWI-AC-132` | Bagian 2A.2; `UAT-30` |
| `FR-RI-157` | Jenis kelamin belum tercatat | `BE-RWI-013` | `RWI-AC-129` | Bagian 2A.1 |
| `FR-RI-158` | Isolasi atribut episode | `BE-RWI-014` | `RWI-AC-136` | Bagian 2A.4 |
| `FR-RI-159` | Catatan awal vs keputusan klinis | `BE-RWI-014`, `FE-RWI-006`, `FE-RWI-009` | `RWI-AC-136`, `RWI-AC-137`, `RWI-AC-139` | Bagian 2A.4; `UAT-32` |
| `FR-RI-160` | Tempat tidur isolasi dijaga dua arah | `BE-RWI-015` | `RWI-AC-134`, `RWI-AC-135` | Bagian 2A.3; `UAT-31` |
| `FR-RI-161` | Perubahan isolasi tidak ditahan; daftar pantau | `BE-RWI-015`, `FE-RWI-016` | `RWI-AC-138` | Bagian 2A.5; `UAT-33` |
| `FR-RI-162` | Berlaku pada perpindahan | `BE-RWI-019` | `RWI-AC-133` | Bagian 2A.1 |

**Enam puluh dua functional requirement, nol tanpa task.**

---

## 3. Arah balik — task → epic

Bagian ini memeriksa kebalikannya: adakah task yang tidak ada yang memintanya?

| Task | Epic yang dilayani | Bila tidak menempel epic, apa dasarnya |
| --- | --- | --- |
| `BE-RWI-001` ✅ s.d. `BE-RWI-004` | `EPIC RI-21` | — |
| `BE-RWI-005` | `EPIC RI-31` | — |
| `BE-RWI-006`, `BE-RWI-032` | `EPIC RI-32` | — |
| `BE-RWI-007` s.d. `BE-RWI-009` | `EPIC RI-21` | — |
| `BE-RWI-010` | `EPIC RI-22` | — |
| `BE-RWI-011`, `BE-RWI-012` | `EPIC RI-23` | — |
| `BE-RWI-013` s.d. `BE-RWI-015` | `EPIC RI-34` | — |
| `BE-RWI-016` | `EPIC RI-24` | — |
| `BE-RWI-017`, `BE-RWI-018` | `EPIC RI-25` | — |
| `BE-RWI-019` | `EPIC RI-26` | — |
| `BE-RWI-020` s.d. `BE-RWI-022` | `EPIC RI-27` | — |
| `BE-RWI-023` s.d. `BE-RWI-027` | `EPIC RI-28` | — |
| `BE-RWI-028`, `BE-RWI-029` | `EPIC RI-29` | — |
| `BE-RWI-030` | `EPIC RI-30` | — |
| `BE-RWI-031` | `EPIC RI-33` | — |
| `BE-RWI-033` | **Tidak ada** | `NFR-008`, `RWI-DEC-051`. Bukti penerimaan lintas epic |
| `FE-RWI-001` | `EPIC RI-32` | — |
| `FE-RWI-002` | **Tidak ada** | Kerangka lintas layar. `03-frontend-architecture.md` bagian 8 |
| `FE-RWI-003`, `FE-RWI-004` | `EPIC RI-31` | — |
| `FE-RWI-005` | `EPIC RI-22`, `EPIC RI-34` | — |
| `FE-RWI-006`, `FE-RWI-007` | `EPIC RI-21`, `EPIC RI-23`, `EPIC RI-34` | — |
| `FE-RWI-008` | `EPIC RI-24` | — |
| `FE-RWI-009` | `EPIC RI-21`, `EPIC RI-34` | — |
| `FE-RWI-010` | `EPIC RI-26` | — |
| `FE-RWI-011` | `EPIC RI-25` | — |
| `FE-RWI-012` s.d. `FE-RWI-015` | `EPIC RI-27`, `EPIC RI-28` | — |
| `FE-RWI-016`, `FE-RWI-017` | `EPIC RI-29`, `EPIC RI-34` | — |
| `FE-RWI-018` | `EPIC RI-30` | — |
| `FE-RWI-019` | **Tidak ada** | `RWI-DEC-051`, `03-frontend-architecture.md` bagian 10 |

**Tiga task tanpa epic, ketiganya beralasan tertulis.** Tidak ada task yatim.

---

## 4. Decision ID → task

Hanya keputusan yang **mengikat implementasi** yang didaftar. Keputusan yang hanya menutup scope
ada pada bagian 6.

| Decision | Isinya | Task yang menegakkannya |
| --- | --- | --- |
| `RWI-DEC-008` | Pemesanan 2 jam, dapat diubah admin | `BE-RWI-005`, `BE-RWI-010` |
| `RWI-DEC-009` | Lima status episode, `InCare` dibuang | `BE-RWI-003`, `BE-RWI-007` |
| `RWI-DEC-010` | Batas pembatalan admisi | `BE-RWI-008` |
| `RWI-DEC-011`, `RWI-DEC-041` | Episode selalu menempel kunjungan | `BE-RWI-007` |
| `RWI-DEC-012` s.d. `RWI-DEC-014` | Kewenangan dan keutuhan perpindahan | `BE-RWI-019` |
| `RWI-DEC-015` | Gerbang keuangan dan jalan keluar supervisor | `BE-RWI-024`, `BE-RWI-026` |
| `RWI-DEC-016`, `RWI-DEC-017` | Keputusan pulang, lima cara pulang | `BE-RWI-020`, `BE-RWI-025` |
| `RWI-DEC-019` | Pasien titipan tidak dikenali; kelas mengikuti kamar | `BE-RWI-019` |
| `RWI-DEC-020` | Bayi punya episode sendiri | `BE-RWI-031` |
| `RWI-DEC-021` | Keadaan tempat tidur diperiksa ulang saat penempatan | `BE-RWI-011` |
| `RWI-DEC-022` s.d. `RWI-DEC-024` | Kewenangan DPJP | `BE-RWI-017`, `BE-RWI-019` |
| `RWI-DEC-026` | Daftar periksa administrasi menahan | `BE-RWI-023` |
| `RWI-DEC-027` | Lama dirawat dari selisih tanggal | `BE-RWI-016` |
| `RWI-DEC-028` | Sesi koreksi | `BE-RWI-030` |
| `RWI-DEC-030` | Kedaluwarsa `Draft` | `BE-RWI-008` |
| `RWI-DEC-032` | Daftar pantau berpenanggung jawab | `BE-RWI-029` |
| `RWI-DEC-033` | Obat pulang sebagai butir daftar periksa | `BE-RWI-023` |
| `RWI-DEC-039` | `MstBed.BedStatus` turun jadi salinan | `BE-RWI-006`, `BE-RWI-011`, `BE-RWI-029` |
| `RWI-DEC-040` | Kelayakan keuangan ditandai manual | `BE-RWI-024` |
| `RWI-DEC-048` | Seeder menolak produksi | `BE-RWI-002` |
| `RWI-DEC-049` | Perbaikan tombol tempat tidur | `FE-RWI-001` |
| `RWI-DEC-051` | Test menempel pada tiap task | Seluruh task; `BE-RWI-032`, `BE-RWI-033`, `FE-RWI-019` |
| `RWI-DEC-053` | Riwayat lokasi milik Rawat Inap | `BE-RWI-009`, `BE-RWI-011` |
| `RWI-DEC-054` | Satu pasien satu episode hadir | `BE-RWI-003`, `BE-RWI-012` |
| `RWI-DEC-055` | Kepergian fisik pasien | `BE-RWI-027`, `FE-RWI-015` |
| `RWI-DEC-056` | Penanda rawat gabung bayi | `BE-RWI-031` |
| `RWI-DEC-057` | Versi resume pulang | `BE-RWI-022` |
| `RWI-DEC-062` | Persetujuan pemilik modul tetangga | `BE-RWI-006` |
| `RWI-DEC-063` | Penanggung jawab data master | Gerbang bagi `BE-RWI-010` ke atas |
| `RWI-DEC-064` | Jenis kelamin dan isolasi **menolak** | `BE-RWI-013`, `BE-RWI-015`, `BE-RWI-019` |
| `RWI-DEC-065` | Isolasi atribut episode | `BE-RWI-003`, `BE-RWI-014`, `FE-RWI-006`, `FE-RWI-009` |
| `RWI-DEC-066` | Seluruh kamar tidak boleh campur, tanpa kolom baru | `BE-RWI-013` |
| `RWI-DEC-069` | Pemilik `EmergencyInstallationManagement` bernama: Rizki Gunawan | Gerbang bagi `INP-S09`; tidak ada task MVP |
| `RWI-DEC-070` | Pelonggaran mesin klinis meluas ke kunjungan `Emergency` | Tidak ada task modul ini — pelaksananya modul IGD lewat `IGD-DEC-068` |
| `RWI-DEC-071` | Justifikasi `RWI-DEC-041` ditulis ulang | Tidak ada task — keputusannya tidak berubah |
| `RWI-DEC-072` | Waktu tiba milik IGD; penempatan menunggu event `Tiba` | `BE-RWI-011` kriteria 7 sebagai penjaga; aturan penuhnya menunggu `INP-S09` |
| `RWI-DEC-073` | `OriginEncounterId` dikerjakan modul IGD | `BE-RWI-003` — menegaskan kriteria 5 tetap utuh; tidak ada pekerjaan kolom di modul ini |
| `RWI-DEC-074` | Blueprint revision `4` disetujui | Gerbang `BLUEPRINT_APPROVED` bagi `roadmap_revision` `2` |

---

## 5. Invariant dan penjaga → task

| Penjaga | Isinya | Ditegakkan oleh | Dibuktikan test |
| --- | --- | --- | --- |
| `INV-INP-01` | Episode aktif punya tepat satu penempatan aktif; dilonggarkan setelah kepergian dicatat | `BE-RWI-011`, `BE-RWI-027` | Bagian 2, 4A |
| `INV-INP-02` | Satu tempat tidur paling banyak satu penempatan aktif | `BE-RWI-003` index parsial, `BE-RWI-011` | Bagian 2 skenario tabrakan |
| `INV-INP-03` | Episode wajib punya DPJP | `BE-RWI-007` | Bagian 1 |
| `INV-INP-04` | Satu kunjungan satu episode | `BE-RWI-007` | Bagian 1 |
| `INV-INP-07` | Perpindahan utuh | `BE-RWI-019` | Bagian 3 |
| `INV-INP-10` | Satu pasien satu episode yang hadir | `BE-RWI-003` index parsial, `BE-RWI-012` | Bagian 1 |
| `GUARD-INP-01` | Perpindahan oleh DPJP aktif | `BE-RWI-017`, `BE-RWI-019` | Bagian 3; `FE-RWI-010`, `FE-RWI-019` |
| `GUARD-INP-02` | Keputusan pulang oleh DPJP aktif | `BE-RWI-020` | Bagian 7 |
| `GUARD-INP-03` | Penandatanganan resume oleh DPJP aktif | `BE-RWI-021` | Bagian 7; `UAT-10` |
| `GUARD-INP-04` | Perubahan isolasi setelah episode aktif hanya DPJP | `BE-RWI-014` | Bagian 2A.4; `FE-RWI-009`, `FE-RWI-019` |

**Empat penjaga tidak dapat dikerjakan mesin hak akses** dan ditulis di dalam service. Keempatnya
punya pasangan test di frontend juga, karena tombol yang pasti ditolak server tidak boleh tampil
aktif di layar.

---

## 6. Yang sengaja tidak ditelusuri, beserta dasarnya

| Yang tidak ada task-nya | Alasan | Decision ID |
| --- | --- | --- |
| Pengkajian, catatan dokter, CPPT, tindakan, visite | Slice di luar scope MVP | `DEC-INP-001` |
| Resep rawat inap dan obat pulang | Terikat konsultasi; di luar scope | `DEC-INP-001` |
| Serah terima IGD ke rawat inap | Di luar scope | `DEC-INP-002` |
| Persetujuan umum rawat inap | Di luar scope, menunggu pemilik hukum | `DEC-INP-003` |
| Pengiriman SATUSEHAT | Di luar scope | `DEC-INP-005` |
| Serah terima klinis antar shift | Di luar scope; isinya menunggu pemilik klinis | `DEC-INP-006`, `RWI-OQ-038` |
| Aturan klinis pasien meninggal dan kabur | Cara pulangnya dikenali sistem, aturan klinisnya menunggu pemilik klinis | `DEC-INP-007`, `RWI-OQ-039`, `RWI-DEC-059` |
| Daftar pantau kepatuhan pengkajian dan CPPT | Bergantung pada slice di luar scope | `DEC-INP-001` |
| Masa simpan riwayat status | Sudah dijawab, menunggu pemilik hukum | `RWI-OQ-035`, `RWI-DEC-060` |
| Tabel riwayat kebutuhan isolasi | Isolasi adalah **atribut**, bukan riwayat | `RWI-DEC-065` |
| Kolom "boleh campur" pada `MstRoom` | Ditolak tegas; diperiksa dari penghuni yang sedang ada | `RWI-DEC-066` |

Sebelas butir, **seluruhnya beralasan tertulis**. Tidak ada satu pun yang berbunyi "menyusul".

---

## 7. Ringkasan kelengkapan

| Yang diperiksa | Jumlah | Tertelusur | Lubang |
| --- | ---: | ---: | ---: |
| Epic | 14 | 14 | **0** |
| Functional requirement | 62 | 62 | **0** |
| Skenario UAT | 33 | 33 | **0** |
| Invariant dan penjaga | 10 | 10 | **0** |
| Decision yang mengikat implementasi | 32 | 32 | **0** |
| Task backend | 33 | 33 | **0** |
| Task frontend | 19 | 19 | **0** |

### Yang **belum** dapat diperiksa sekarang

| Butir | Kenapa belum | Kapan dapat diperiksa |
| --- | --- | --- |
| 139 acceptance criteria → berkas test yang benar-benar ada | Belum ada satu pun berkas test Rawat Inap di repository | `BE-RWI-033` |
| 49 endpoint baru → status tersedia pada api contract | Seluruhnya masih "Rencana (belum tersedia)". Baris ke-50 adalah perubahan perilaku pada endpoint yang sudah ada, dinilai terpisah | `BE-RWI-033` |
| Cakupan e2e frontend | Frontend baru punya empat berkas test, tidak satu pun menyentuh Rawat Inap | `FE-RWI-019` |

Ketiganya adalah **konsekuensi wajar** dari modul yang belum satu baris pun ditulis, bukan lubang
perencanaan. Keduanya punya task penutup yang memeriksanya.

---

## 8. Gerbang sebelum roadmap ini boleh dijalankan

| Gerbang | Keadaannya |
| --- | --- |
| ~~**Approval blueprint**~~ | **DICABUT 24 Agustus 2026** oleh `RWI-DEC-067`. Disetujui Muhammad Hamzah; `approved_by` pada metadata sudah terisi |
| Kesiapan data master beserta penanda yang benar | `RWI-DEC-063`, target 22 Agustus 2026. Menahan `BE-RWI-010` ke atas, **tidak** menahan `BE-RWI-001` s.d. `BE-RWI-004` |
| `FE-RWI-001` sebelum `BE-RWI-006` | Lintas repository, wajib diurutkan |
| ~~Registry lifecycle `PLANNED`~~ | **DICABUT 24 Agustus 2026** oleh `RWI-DEC-068`. Modul naik `PLANNED` → `ACTIVE` |
| Tidak ada connection string lokal | Baru — ditemukan saat `BE-RWI-001`. Menahan **cara aman** menjalankan migration, bukan penulisan kodenya |

Ketiga gerbang produksi pada `blueprint-manifest.md` bagian 7.2 — masa simpan data,
interoperabilitas nasional, dan persetujuan pasien — **tidak** menahan pengerjaan MVP. Yang
tertahan olehnya hanya kesiapan melayani pasien sungguhan.
