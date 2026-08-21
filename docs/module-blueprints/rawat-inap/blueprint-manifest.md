# Rawat Inap — Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `RWI-BP-001` |
| `revision` | `2` |
| `status` | `draft` — **belum disetujui manusia** |
| `module` | `rawat-inap` / `InPatientManagement`, prefix entity `Inp` |
| `registry_lifecycle` | `PLANNED` — hanya memberi hak penamaan, belum memberi izin implementasi |
| `design_snapshot_at` | `2026-08-21`, revision `2` pada hari yang sama |
| `backend_commit_sha` | `5afb54bd75281648010e50ef14f43ca1f80d8efd` (branch `MHamzah`) |
| `frontend_commit_sha` | `dec4fdeff07c3c96ad9f07f41f184c54cf771371` (branch `HamzahV2`) |
| `owners` | Product/Domain: pemilik suite skill sebagai **pemegang sementara** sesuai `RWI-DEC-006`, nama belum diisi. Clinical governance: `OPEN`. Security/Privacy: `OPEN`. API dan Frontend authority: sesuai decision log |
| `approved_by` | **Belum ada.** Approval adalah tindakan manusia |
| `approved_at` | — |
| `requirement_readiness` | `PARTIALLY_READY` |
| `domain_architecture_revision` | `0.1` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_PARTIAL` |
| `scope` | Sembilan slice yang dinyatakan siap dan berdiri sendiri. Delapan slice lain sengaja tidak dirancang |
| `compatibility_impact` | **Tiga belas** tabel baru, **nol** perubahan kolom pada tabel modul lain. Satu perubahan **perilaku** pada `PATCH /beds/{id}/availability`. Satu perbaikan pemanggilan di frontend |

---

## 0. Status kesegaran

**Blueprint sudah sejalan dengan masukannya.** Revision `2` memasukkan empat keputusan Amendment
Pass 2026-08-21 yang sebelumnya membuat revision `1` tertinggal.

| Keputusan | Sudah masuk ke |
|---|---|
| `RWI-DEC-054` satu pasien satu episode yang hadir | `02-backend-architecture.md` §1.3 dan §1.5, `erd/`, `contracts/validation-matrix.md`, `testing/`, `04-prd-to-mvp.md` `FR-RI-148` |
| `RWI-DEC-055` kepergian fisik pasien | Seluruh berkas. Endpoint baru, kolom baru, nilai enum baru, `INV-INP-01` dilonggarkan |
| `RWI-DEC-056` penanda rawat gabung bayi | `erd/`, `contracts/validation-matrix.md`, `04-prd-to-mvp.md` `FR-RI-152` |
| `RWI-DEC-057` versi resume pulang | Tabel `InpDischargeSummaryRevision` beserta ERD, kamus data, DDL, dan `FR-RI-153` |

`RWI-DEC-053` sengaja tidak mengubah apa pun: riwayat lokasi tetap dimiliki `InpBedPlacement`.

### 0.1 Satu artefak hulu yang kini tertinggal

[`evidence/03-hospital-domain-architecture.md`](./evidence/03-hospital-domain-architecture.md)
masih revision `0.1` dan **belum** memuat perubahan Amendment Pass. Yang perlu diselaraskan:

| Bagian | Yang perlu diperbarui |
|---|---|
| `INV-INP-01` | Dilonggarkan untuk episode `DischargePending` yang kepergiannya sudah dicatat |
| Invariant baru `INV-INP-10` | Satu pasien satu episode yang benar-benar hadir |
| `CMD-INP-15` | Perintah bisnis baru: catat pasien sudah meninggalkan ruangan |
| `ARCH-GAP-002` s.d. `ARCH-GAP-005` | Keempatnya sudah tertutup oleh `RWI-DEC-054` s.d. `RWI-DEC-057` |

**Ini tidak diselesaikan di sini dengan sengaja.** Bounded context, batas aggregate, invariant, dan
lifecycle adalah wewenang `/hospital-domain-architect`; skill penyusun blueprint dilarang
merancangnya ulang. Yang dilakukan revision `2` hanyalah **menerapkan keputusan pemilik** yang sudah
tercatat pada decision log, bukan mengarang konsep domain baru.

Selisih ini **tidak memblokir** pemakaian blueprint, karena isi blueprint dan decision log sudah
sejalan. Yang tertinggal hanya catatan arsitektur domainnya.

---

## 1. Peringatan sebelum membaca

Seluruh dokumen pada folder ini berstatus `draft`. Tidak satu pun boleh dipakai sebagai izin
menulis source code.

Empat gerbang implementasi masih terbuka, dan modul `InPatientManagement` masih berstatus `PLANNED`
pada registry.

---

## 2. Daftar artefak dan hash

| Artefak | Revision | Status | SHA-256 |
|---|---|---|---|
| [`00-interview-decisions.md`](./00-interview-decisions.md) | `4` | `draft` | `a090ab768e9f5002926760bde904ca72ffd95811917251438e232c523d707316` |
| [`01-existing-capability-map.md`](./01-existing-capability-map.md) | `1.2` | `source-audited` | `567d7f7ea57537f419efca28d551e965524d27ea1889a00cc7707d17ec74c3b6` |
| [`evidence/02-requirement-completeness-gate.md`](./evidence/02-requirement-completeness-gate.md) | `1.0` | `CURRENT` | `cc32db172b2441b2967ce3507c89b81f12fc103bbd3b3a92bc7bc49d77005ffe` |
| [`evidence/03-hospital-domain-architecture.md`](./evidence/03-hospital-domain-architecture.md) | `0.1` | `draft` | `721268f11edd4aff047b6fcf03fce28e4f051cb4d1cf5134c32d11f0f52615d3` |
| [`02-backend-architecture.md`](./02-backend-architecture.md) | `0.2` | `draft` | `c4dca4a47d58e4f85cdfb76d11e946f1defa1d360b36455b186064f69390bb2c` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `0.2` | `draft` | `79f27cf0f5c64e6452b4dbf8b83103e281dcf97145187d99f6d3e687e435a47a` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | `0.2.0` | `draft` | `a37cc53339878e6baf901632d18e133812a379e08db0035a520d85e6cd9e4f29` |
| [`erd/00-context-erd.md`](./erd/00-context-erd.md) | `0.1` | `draft` | `c3b55556ae1b1d3115628e091162f53aad2aa92bbeb699b41dd2ea1dde232cf3` |
| [`erd/01-inpatient-episode.md`](./erd/01-inpatient-episode.md) | `0.2` | `draft` | `1928c2697f5f53054ad2d19d86324ad0fb1cbd0f67d2d4ca8a29b4e33d73bb69` |
| [`erd/02-inpatient-configuration.md`](./erd/02-inpatient-configuration.md) | `0.1` | `draft` | `3645ee9d1788270ee7cef88d2cc6b74beddddec0a1a5d2b538e45c25c66f2065` |
| [`erd/data-dictionary.md`](./erd/data-dictionary.md) | `0.2` | `draft` | `144c018d686ed6bbe05c3ef89cf5399fee900a47fcb7f0ba2e96638de828fcf0` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | `0.2.0` | `draft` | `a88e62be884f15a1d9e6f17b3d2ccc6252929f237e6354ec0da3cfab0e41ccf2` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | `0.2.0` | `draft` | `ea77460d23f696c41e97a7d5d3b84cf48f60a2b56fa8800477daeb4606e676e7` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `0.2.0` | `draft` | `8aad790cd3ff971e9bbe7fbd9548ed6ccd5f84e5e210ed8ad9135ce51298db37` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | `0.2.0` | `draft` | `9bd86818e421bd3a48c2d8dca4015c8ee08381092e61f5bc379bbb4ad89ee4b3` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | `0.2.0` | `draft` | `d560506f5a6fb94d018938890a31b8df37246f05171e72ec4bcadf0edfd82a63` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | `0.2.0` | `draft` | `cf1692b392445eb34190f88ce7619c3b0de49d26c7f1fd6dc4cd45c49029e54f` |

Hash di atas dipakai mendeteksi perubahan yang tidak tercatat. Bila salah satu berubah tanpa
revision naik, blueprint dianggap tidak konsisten.

---

## 3. Contract version

| Kontrak | Version | Status |
|---|---|---|
| API | `0.2.0` | `draft` |
| State transition | `0.2.0` | `draft` |
| Validation | `0.2.0` | `draft` |
| Integration | `0.2.0` | `draft` |
| Permission dan audit | `0.2.0` | `draft` |
| Acceptance test | `0.2.0` | `draft` |
| PRD ke MVP | `0.2.0` | `draft` |

---

## 4. Rantai masukan

```text
00-interview-decisions.md  rev 3  (grill-me: Scope + Closure + Amendment Pass)
        |
01-existing-capability-map.md  (trace-existing-capabilities)
        |
evidence/02-requirement-completeness-gate.md  (PARTIALLY_READY)
        |
evidence/03-hospital-domain-architecture.md  (DOMAIN_ARCHITECTURE_PARTIAL)
        |
02-backend-architecture.md + 03-frontend-architecture.md + erd/ + contracts/ + testing/
        |
04-prd-to-mvp.md
```

Setiap tahap hanya meneruskan slice yang dinyatakan siap oleh tahap sebelumnya.

---

## 5. Scope yang dirancang

| Slice | Nama | Epic |
|---|---|---|
| `INP-S01` | Admisi dan pemesanan tempat tidur | `EPIC RI-21`, `EPIC RI-22` |
| `INP-S02` | Penempatan, census, lama dirawat | `EPIC RI-23`, `EPIC RI-24` |
| `INP-S03` | Perpindahan dan pindah kelas | `EPIC RI-26` |
| `INP-S04` | Penugasan perawat | `EPIC RI-25` |
| `INP-S07` sebagian | Keputusan pulang dan resume, tiga cara pulang | `EPIC RI-27` |
| `INP-S08` sebagian | Daftar periksa, kelayakan keuangan, penutupan | `EPIC RI-28` |
| `INP-S12` | Bayi baru lahir dan boks bayi | `EPIC RI-33` |
| `INP-S13` | Riwayat status, audit, dua daftar pantau | `EPIC RI-29`, `EPIC RI-30` |
| `INP-S14` | Pengaturan yang dapat diubah admin | `EPIC RI-31` |
| — | Perbaikan tempat tidur dan pembatasan wewenang status | `EPIC RI-32` |

## 6. Scope yang sengaja tidak dirancang

| Slice | Decision ID |
|---|---|
| `INP-S05` dokumentasi klinis dan visite | `DEC-INP-001` |
| `INP-S06` resep dan obat pulang | `DEC-INP-001` |
| `INP-S09` serah terima IGD | `DEC-INP-002` |
| `INP-S10` persetujuan umum | `DEC-INP-003` |
| `INP-S11` jenis kelamin dan isolasi | `DEC-INP-004` |
| `INP-S15` interoperabilitas SATUSEHAT | `DEC-INP-005` |
| Serah terima klinis antar shift | `DEC-INP-006` |
| Cara pulang meninggal dan kabur | `DEC-INP-007` |

---

## 7. Design gate

Blueprint ini adalah desain target, bukan spesifikasi implementasi yang disetujui.

### 7.1 Gerbang implementasi — desain boleh, source code belum

| Gate | Keterangan |
|---|---|
| Persetujuan pemilik modul tetangga | Empat modul `ACTIVE` akan disentuh. Lihat `RWI-OQ-032`, `RWI-OQ-033`, `RWI-OQ-034` |
| Kesiapan data master | Kamar dan tempat tidur harus terisi lewat layar aplikasi. Lihat `RWI-DEC-048`, `RWI-OQ-036` |
| Perbaikan tombol tempat tidur | Hari ini selalu gagal 404. Lihat `RWI-DEC-049` |
| Test regresi modul tetangga | Tidak ada satu pun test yang menjaga jalur poliklinik, IGD, dan farmasi. Lihat `RWI-DEC-051` |
| Registry lifecycle | Modul masih `PLANNED` |

### 7.2 Gerbang sebelum produksi — klinis dan privasi

| Gate | Keterangan |
|---|---|
| Clinical governance owner | Belum ditunjuk |
| Security/privacy owner | Belum ditunjuk |
| `RWI-RULE-012` isolasi dan jenis kelamin | Gerbang keras. `DEC-INP-004` |
| `RWI-RULE-021` batas waktu klinis | Gerbang keras |
| `RWI-RULE-025` persetujuan umum | Gerbang keras. `DEC-INP-003` |
| Masa simpan riwayat | `RWI-OQ-035`, keputusan hukum |

---

## 8. Yang tidak boleh diubah blueprint hilir

Diwariskan dari arsitektur domain bagian N.5. Perubahan pada butir berikut wajib kembali ke skill
hulu, bukan diselesaikan pada tahap perencanaan atau implementasi:

1. Kepemilikan data pada tabel kepemilikan `02-backend-architecture.md` bagian 2.
2. Kedudukan `MstBed.BedStatus` sebagai **salinan**, bukan sumber kebenaran.
3. Sepuluh invariant `INV-INP-01` sampai `INV-INP-09` beserta cara menjaganya.
4. Bentuk **berperiode** pada `InpDoctorAssignment`, `InpNurseAssignment`, dan `InpBedPlacement`.
   Menggantinya dengan satu kolom yang ditimpa akan menghapus riwayat yang dibutuhkan resume,
   billing, dan interoperabilitas.
5. Kedudukan `InpCorrectionSession` sebagai konsep tersendiri, bukan status episode keenam.

---

## 9. Pemicu impact scan

Blueprint ditandai stale dan wajib melewati impact scan bila salah satu berikut berubah:

| Yang berubah | Yang harus ditinjau ulang |
|---|---|
| `backend_commit_sha` atau `frontend_commit_sha` | Capability map lebih dulu, lalu seluruh kontrak |
| `Areas/HealthServices/MasterData/` tempat tidur, kamar, unit layanan, kelas | `erd/`, `contracts/api-contract.md`, `EPIC RI-22`, `RI-23`, `RI-32` |
| `Areas/HealthServices/RegistrationManagement/` | `INV-INP-04`, `EPIC RI-21` |
| `Areas/HealthServices/ClinicalManagement/` atau `PharmacyManagement/` | `DEC-INP-001`; slice yang dihentikan mungkin dapat dibuka |
| `Areas/HealthServices/BillingManagement/` | `RWI-RULE-028` aturan 7; sumber kelayakan keuangan mungkin berpindah |
| `Repositories/ApplicationDbContext.cs` | Rencana migration |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Prefix dan lifecycle modul |
| Munculnya berkas berawalan `Inp` | Seluruh status `Baru` wajib dinilai ulang |

---

## 10. Riwayat revision

| Revision | Tanggal | Ringkasan |
|---|---|---|
| `2` | 2026-08-21 | Menyerap empat keputusan Amendment Pass. Satu tabel baru `InpDischargeSummaryRevision`, tiga kolom baru pada `InpEpisode`, satu nilai enum baru, satu endpoint baru, satu invariant baru `INV-INP-10`, dan `INV-INP-01` dilonggarkan. 6 functional requirement baru, 5 skenario UAT baru, 23 skenario acceptance test baru. Tidak ada kemampuan `MUST HAVE` yang dicabut dan tidak ada epic baru |
| `1` | 2026-08-21 | Blueprint pertama. Dua bounded context, satu aggregate root, dua belas tabel baru, nol perubahan kolom pada tabel modul lain. Sembilan slice dirancang, delapan sengaja dihentikan. 13 epic, 47 functional requirement, 23 skenario UAT, 82 skenario acceptance test |

---

## 11. Langkah berikutnya

| Kondisi | Skill |
|---|---|
| Empat pertanyaan memblokir pada `04-prd-to-mvp.md` bagian 20.2 sudah terjawab dan owner menyetujui blueprint | `/qv-plan` |
| Tujuh pertanyaan tidak memblokir ingin ditutup lebih dulu | `/qv-grill` Amendment Pass |
| Salah satu SHA berubah | `/qv-trace` impact scan |
| Slice yang dihentikan ingin dibuka | Tutup Decision ID-nya lebih dulu lewat `/qv-grill`, lalu ulangi dari `requirement-completeness-gate` |

**Blueprint ini `MUST NOT` diteruskan ke `/qv-plan` sebelum empat pertanyaan memblokir terjawab.**
