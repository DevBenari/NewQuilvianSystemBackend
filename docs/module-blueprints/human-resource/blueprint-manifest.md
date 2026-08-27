# Human Resource — Blueprint Manifest

```yaml
blueprint_id: HRD-BP-001
module_name: Human Resource
module_slug: human-resource
module_prefix: HRD
revision: 4
status: DRAFT
design_readiness: PARTIAL
current_phase: HRD-PH-001
created_at: 2026-08-27T07:02:01Z
updated_at: 2026-08-27T07:58:00Z
last_verified_at: 2026-08-27T07:58:00Z
backend_source_sha_audited: ecdc135444f0110482c9702212bcea30043983c8
backend_source_sha_verified: 16b8b71f4cd61e083213cf90722f4d768d339739
frontend_source_sha: 2a1cea7841a4433f8637d486204e60314c09d131
skill_suite_version: 1.0.0-rc2
input_revision_hash: 67c182c936c9d3c1dcd44f55537cd967f3dcb3824d8ff2dd1b66bf6533e89b5d
decision_revision: 5
backend_baseline_branch: QuilvianIntegrationBackend
capability_map_revision: 1.1
capability_map_hash: 0bcad6fb727b9bccd50abe053e23924e49962bd915083cb50f8bcdefa2171c1f
contract_versions: []
active_dependency_ids:
  - HRD-DEP-001
  - HRD-DEP-002
  - HRD-DEP-003
  - HRD-DEP-004
  - HRD-DEP-005
  - HRD-DEP-006
  - HRD-DEP-007
active_roadmap_revision: 2
supersedes: null
```

---

## 1. Cara membaca manifest ini

| Field | Arti dalam bahasa sehari-hari |
| --- | --- |
| `blueprint_id` | Nama tetap blueprint ini. Tidak pernah berubah |
| `revision` | Naik hanya bila arsitektur target, kontrak, dependency, atau keputusan yang disetujui berubah secara material. Tidak naik hanya karena status berubah |
| `status` | Keadaan blueprint sebagai dokumen. Masih `DRAFT` karena belum ada approval manusia atas keseluruhannya |
| `design_readiness` | Keadaan modul dilihat dari boleh tidaknya dirancang. `PARTIAL` karena sebagian siap dan sebagian terblokir |
| `input_revision_hash` | Sidik jari `00-interview-decisions.md` yang menjadi dasar revision ini |
| `capability_map_hash` | Sidik jari `01-existing-capability-map.md` yang menjadi dasar revision ini |
| `contract_versions` | Masih kosong karena kontrak API, integrasi, dan state belum ditulis. Diisi pada fase berikutnya |

`status: DRAFT` dan `design_readiness: PARTIAL` menjawab dua pertanyaan berbeda. Yang pertama:
apakah dokumen ini sudah disetujui. Yang kedua: apakah isinya boleh dipakai untuk merancang.
Sebuah blueprint bisa saja `DRAFT` tetapi sebagian besarnya sudah `READY FOR DESIGN`, dan itu
persis keadaan modul ini.

---

## 2. Klasifikasi kesiapan desain

Empat nilai yang dipakai konsisten di seluruh blueprint HR:

| Nilai | Artinya | Jumlah kelompok kemampuan |
| --- | --- | ---: |
| `READY FOR DESIGN` | Boleh dirancang sampai final | 15 |
| `PARTIAL` | Sebagian boleh final, sebagian tertahan. Batasnya wajib disebut eksplisit | 1 |
| `BLOCKED` | Tidak boleh dirancang sama sekali | 6 |
| `DEFERRED` | Boleh secara teknis, sengaja ditunda karena prioritas | 1 |

**Aturan yang mengikat manifest ini:** nilai `BLOCKED` tidak boleh diubah menjadi
`READY FOR DESIGN` tanpa dependency-nya benar-benar terpenuhi dan buktinya dicatat pada
`01-prerequisite-readiness.md`. Menaikkan status berdasarkan asumsi, kemiripan dengan modul
lain, atau desakan jadwal adalah pelanggaran terhadap manifest ini.

Rincian per kelompok kemampuan ada di [`MODULE-STATUS.md`](./MODULE-STATUS.md) bagian 2.

---

## 3. Kepemilikan

| Peran | Pemilik | Dasar |
| --- | --- | --- |
| Keputusan rekayasa dan produk teknis | Pengguna | `HRD-DEC-015`, `approved` 2026-08-27 |
| Kebijakan bisnis HR | `OPEN` | `HRD-Q-01` masih terbuka |
| Kewenangan klinis dan keselamatan pasien | Komite Medik — belum ada wakil yang ditunjuk | `HRD-Q-08` |
| Kesehatan dan keselamatan kerja | K3RS — belum ada wakil yang ditunjuk | `HRD-DEC-010` masih `draft` |
| Batas payroll dengan Finance | Pemilik produk bersama Finance | `HRD-Q-10`, `HRD-Q-11` |
| Kewenangan API | Backend, mengikuti `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` | `AGENTS.md` |
| Kewenangan frontend | Pengguna, dengan ruang `DEV_DISCRETION` sesuai `HRD-FE-03` | `HRD-DEC-007`, `HRD-DEC-015` |

`approved_by` dan `approved_at` untuk blueprint secara keseluruhan: **belum ada**. Yang sudah
ada adalah approval per keputusan, tercatat pada `00-interview-decisions.md`.

---

## 4. Dependency aktif

| ID | Ringkasan | Jenis | Status kemampuan |
| --- | --- | --- | --- |
| `HRD-DEP-001` | Registry kepemilikan dan prefix modul | `MODULE_FOUNDATION` | `REPAIR` — `Wfp` belum terdaftar |
| `HRD-DEP-002` | Mesin workflow dan persetujuan bersama | `MODULE_FOUNDATION` | `EXTEND` |
| `HRD-DEP-003` | Identitas dan hak akses aplikasi | `INTEGRATION` | `UNKNOWN` |
| `HRD-DEP-004` | Finance untuk penyelesaian payroll | `INTEGRATION` | `UNKNOWN` |
| `HRD-DEP-005` | Health Services untuk pengecekan kewenangan klinis | `INTEGRATION` | `UNKNOWN` |
| `HRD-DEP-006` | Penyimpanan berkas dan dokumen | `EXTERNAL` | `UNKNOWN` |
| `HRD-DEP-007` | Arsitektur domain rumah sakit untuk slice klinis | `PHASE` | `MISSING` |

Rincian lengkap beserta bukti dan dampak pemblokirannya ada di
[`01-prerequisite-readiness.md`](./01-prerequisite-readiness.md).

---

## 5. Keputusan yang mengikat seluruh artefak

Sembilan keputusan berikut berlaku di setiap dokumen blueprint HR dan tidak boleh ditafsir ulang
per artefak.

| Decision ID | Isi yang mengikat | Status |
| --- | --- | --- |
| `HRD-DEC-003` | Satu blueprint utuh untuk 21 capability; batas rilis ditulis di `04-prd-to-mvp.md` | `approved` |
| `HRD-DEC-004` | Otoritas skema hybrid: domain berjalan dikunci, domain tanpa controller diturunkan ulang | `approved` |
| `HRD-DEC-005` | Kredensial kedaluwarsa memberi peringatan tercatat, tidak menghentikan pelayanan | `draft` — menunggu Komite Medik |
| `HRD-DEC-009` | Tanggung jawab HR atas payroll berhenti setelah `execute` | `approved` |
| `HRD-DEC-010` | Rekam kesehatan kerja hanya untuk K3RS dan pegawai bersangkutan | `draft` — menunggu K3RS |
| `HRD-DEC-016` | Kebab-case sebagai route canonical, route lama tetap hidup sebagai compatibility alias. **Bukan** hard breaking rename | `approved` |
| `HRD-DEC-017` | ~~`Hrd` sebagai target naming; migrasi legacy bertahap per domain sebagai campaign~~ | `superseded` oleh `HRD-DEC-019` |
| `HRD-DEC-018` | Satu UX kotak masuk; workflow, policy, permission, validasi, SLA, dan eskalasi tetap per jenis transaksi | `approved` |
| `HRD-DEC-019` | **Kebijakan penamaan canonical HR.** `Mst` tetap master/reference. `Wfp` prefix yang sah untuk keluarga workforce, **bukan** legacy. `Hrd` canonical dan default untuk entity operasional HR baru. `Trx` legacy generik dengan **ratchet hanya saat materially touched** | `approved` |

`HRD-DEC-016` menggantikan `HRD-DEC-014`. `HRD-DEC-017` menggantikan `HRD-DEC-008`, lalu
`HRD-DEC-017` sendiri digantikan `HRD-DEC-019`. Seluruhnya tercatat `superseded` pada decision
log dan tidak dihapus.

### 5.1 Matriks prefix yang berlaku

| Prefix | Arti | Entity baru boleh? | Kebijakan untuk yang sudah ada |
| --- | --- | :---: | --- |
| `Mst` | Data master atau referensi | Ya, bila memang master/reference | Tidak diubah |
| `Wfp` | Keluarga workforce dan profil HR | Ya, bila memang bagian keluarga itu | Tidak diubah. **Bukan** legacy yang akan dihapus |
| `Hrd` | Entity operasional HR — canonical dan default | Ya, ini pilihan bawaan | Tetap `Hrd` |
| `Trx` | Prefix generik warisan | **Tidak** | Dibiarkan berjalan; menjadi `Hrd*` hanya saat materially touched |

Definisi *material touch* selengkapnya ada di `00-interview-decisions.md` bagian 16.5. Ringkasnya:
yang memicu ratchet adalah perubahan pada entity, konfigurasi EF, tabel atau kolom, relasi,
index, lifecycle persistence, atau migration yang mengenai entity itu. Pekerjaan frontend,
pembacaan data, dokumentasi, perubahan tampilan, dan perbaikan bug yang tidak mengubah kontrak
persistence **tidak** memicu ratchet.

---

## 6. Larangan yang berlaku sepanjang blueprint

1. **Jangan mengarang bounded context klinis.** Kredensial, kewenangan klinis, SPK/RKK, OPPE,
   FPPE, dan kesehatan kerja menunggu `requirement-completeness-gate` dan
   `hospital-domain-architect`.
2. **Jangan memfinalkan bentuk serah terima payroll** sebelum `HRD-Q-10` dan `HRD-Q-11` dijawab
   bersama Finance.
3. **Jangan mengambil keputusan skema yang merusak data** sebelum `HRD-Q-05` dijawab dengan
   audit database yang sebenarnya.
4. **Jangan melakukan rename massal, dan jangan membuat kampanye migration yang mengejar seluruh
   `Trx*` sekaligus.** `HRD-DEC-019` hanya mengizinkan ratchet saat entity itu benar-benar
   disentuh, mengikuti cakupan task yang sedang dikerjakan.
5. **Jangan mengubah `Wfp*` menjadi `Hrd*`** hanya karena namanya berbeda. `Wfp` adalah prefix
   yang sah dan tetap dipakai.
6. **Jangan mengubah `Mst*` menjadi `Hrd*`** hanya karena entity-nya berada di domain HR.
7. **Jangan menetapkan tenggat pembersihan seluruh `Trx*`.** Tidak ada target pembersihan
   menyeluruh.
8. **Jangan membuat entity transaksional HR baru dengan prefix `Trx`.**
9. **Jangan mematikan route lama.** `HRD-DEC-016` menuntut alias tetap hidup sampai audit
   consumer selesai dan masa deprecation berakhir.
10. **Jangan menyimpulkan kepemilikan entity dari prefix.** Empat puluh entity `Trx*` adalah
    milik Health Services, bukan HR.
11. **Jangan memutus proses bisnis yang sedang berjalan** hanya demi konsistensi penamaan.
12. **Blueprint tidak memberi wewenang implementasi.** Tidak ada source aplikasi, migration,
    controller, entity, frontend, maupun database yang boleh disentuh dari alur blueprint ini.

---

## 7. Riwayat revision

| Revision | Tanggal | Isi | Dasar |
| --- | --- | --- | --- |
| `1` | 2026-08-27 | Manifest pertama. Menyerap decision log revision `2` dan capability map revision `1.1`. Scaffold fondasi dan roadmap slice | `HRD-DEC-003` s.d. `HRD-DEC-018` |
| `2` | 2026-08-27 | Amendment Pass 1.1. Menyerap `HRD-DEC-019` kebijakan penamaan canonical yang menggantikan `HRD-DEC-017`. Memperbaiki hitungan slice dan definisi angka 68/67 | `HRD-DEC-019` |
| `3` | 2026-08-27 | Baseline Impact Gate. Baseline backend berpindah ke `origin/QuilvianIntegrationBackend` dengan hasil `NO_IMPACT`; capability map tetap `CURRENT`. Koreksi kelas QBE `S-C4` per entity, revision decision log, dan jumlah flow | `HRD-Q-16`, `HRD-Q-17` |
| `4` | 2026-08-27 | PHASE 2A. Lima flow inti administratif ditulis. `HRD-DEC-020` provenance masukan produk dan `HRD-DEC-021` baseline implementasi canonical. Enam belas pertanyaan baru `HRD-Q-18` s.d. `HRD-Q-33` |

Perubahan setelah approval membuat revision baru dan memicu impact scan pada kedua repository.
Blueprint ini tidak pernah ditandai `approved` oleh skill; approval tetap tindakan manusia.
